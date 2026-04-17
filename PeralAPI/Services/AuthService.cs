namespace PeralAPI.Services
{
    using MongoDB.Driver;
    using PeralAPI.Database;
    using PeralAPI.Models;
    using PeralAPI.Models.DTOs;
    public class AuthService
    {
        private readonly MongoDbContext _db;
        private readonly JwtService _jwt;
        private readonly IConfiguration _config;

        public AuthService(MongoDbContext db, JwtService jwt, IConfiguration config)
        {
            _db = db;
            _jwt = jwt;
            _config = config;
        }

        public async Task<(AuthResponseDto? Result, string Error)> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users
                .Find(u => u.UserName == dto.UserName && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null)
                return (null, "User not found.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return (null, "Incorrect password.");

            return (await IssueTokensAsync(user), string.Empty);
        }

        public async Task<AuthResponseDto?> RefreshAsync(string refreshToken)
        {
            var stored = await _db.RefreshTokens
                .Find(r => r.Token == refreshToken)
                .FirstOrDefaultAsync();

            if (stored == null || !stored.IsActive) return null;

            var user = await _db.Users
                .Find(u => u.Id == stored.UserId && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null) return null;

            // Rotate — revoke old token
            await _db.RefreshTokens.UpdateOneAsync(
                r => r.Id == stored.Id,
                Builders<RefreshToken>.Update.Set(r => r.RevokedAt, DateTime.UtcNow));

            return await IssueTokensAsync(user);
        }

        public async Task<bool> RevokeAsync(string refreshToken)
        {
            var result = await _db.RefreshTokens.UpdateOneAsync(
                r => r.Token == refreshToken && r.RevokedAt == null,
                Builders<RefreshToken>.Update.Set(r => r.RevokedAt, DateTime.UtcNow));

            return result.ModifiedCount > 0;
        }

        private async Task<AuthResponseDto> IssueTokensAsync(User user)
        {
            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            await _db.RefreshTokens.InsertOneAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(
                    int.Parse(_config["Jwt:RefreshTokenExpiryDays"]!))
            });

            return new AuthResponseDto(accessToken, refreshToken, user.Id, user.UserName, user.AvatarUrl, user.Roles);
        }
    }
}
