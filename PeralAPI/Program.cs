using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Hubs;
using PeralAPI.Infrastructure.Swagger;
using PeralAPI.Models;
using PeralAPI.Models.Inventory;
using PeralAPI.Services;
using PeralAPI.Services.Billing;
using PeralAPI.Services.Dashboard;
using PeralAPI.Services.Inventory;
using StackExchange.Redis;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT access token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    //options.EnableAnnotations();
#warning Enable annotations as a improvment
    options.SupportNonNullableReferenceTypes();
    options.UseAllOfToExtendReferenceSchemas();
    options.UseInlineDefinitionsForEnums();
    options.SchemaFilter<RequireNonNullablePropertiesSchemaFilter>();
});


// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
// Redis + SignalR
var redisConnection = builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnection));

    builder.Services.AddSignalR().AddStackExchangeRedis(redisConnection);
}
else
{
    builder.Services.AddSignalR();
}

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
if (jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key must be at least 32 characters. Set it via the Jwt__Key environment variable.");


builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];

                if (!string.IsNullOrEmpty(token) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", p => p.RequireRole("Admin", "Manager"));
});

// Rate limiting — applied to auth endpoints via [EnableRateLimiting("auth")]
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// CORS — origins are configured via Cors:AllowedOrigins in appsettings.json or
// environment variables (e.g. Cors__AllowedOrigins__0=https://yourapp.vercel.app)
var corsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowCredentials()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
// UseHttpsRedirection is opt-in via config — disable when TLS is terminated
// at the load balancer (Railway, Render, etc.) to avoid redirect loops.
if (builder.Configuration.GetValue<bool>("UseHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.Append("X-Frame-Options", "DENY");
    ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseCors("AllowReactApp");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

// Seed Admin
await SeedAdmin(app);
await SeedReservedVendors(app);

app.Run();

static async Task SeedReservedVendors(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

    var exists = await db.Vendors
        .Find(v => v.Name == "Stock Adjustment" && v.IsReserved)
        .AnyAsync();

    if (!exists)
    {
        await db.Vendors.InsertOneAsync(new VendorModel
        {
            Name = "Stock Adjustment",
            IsReserved = true,
            Contacts = new List<ContactModel>()
        });
    }
}

static async Task SeedAdmin(WebApplication app)
{
    var adminPassword = app.Configuration["Seed:AdminPassword"];
    if (string.IsNullOrWhiteSpace(adminPassword))
        throw new InvalidOperationException("Seed:AdminPassword is not configured.");

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();

    var adminExists = await db.Users
        .Find(u => u.Roles.Contains("Admin"))
        .AnyAsync();

    if (!adminExists)
    {
        await db.Users.InsertOneAsync(new User
        {
            UserName = "Admin",
            Email = "admin@local.app",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Roles = new List<string> { "Admin" },
            IsActive = true,
            AvatarUrl = "https://heroui-assets.nyc3.cdn.digitaloceanspaces.com/avatars/orange.jpg"
        });
    }
}