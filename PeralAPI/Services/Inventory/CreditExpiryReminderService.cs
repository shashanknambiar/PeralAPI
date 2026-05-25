using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;

namespace PeralAPI.Services.Inventory
{
    public class CreditExpiryReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<CreditExpiryReminderService> _logger;

        public CreditExpiryReminderService(IServiceScopeFactory scopeFactory, ILogger<CreditExpiryReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending credit expiry reminders.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task SendRemindersAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

            var cutoffDate = DateTime.UtcNow.AddDays(7);

            var filter = Builders<InventoryOrderModel>.Filter.And(
                Builders<InventoryOrderModel>.Filter.Eq(o => o.Status, InventoryOrderStatus.Received),
                Builders<InventoryOrderModel>.Filter.Lte(o => o.CreditExpiryDate, cutoffDate),
                Builders<InventoryOrderModel>.Filter.Eq(o => o.CreditExpiryReminderSentAt, null)
            );

            var orders = await db.InventoryOrders.Find(filter).ToListAsync();

            foreach (var order in orders)
            {
                var vendor = await db.Vendors
                    .Find(Builders<VendorModel>.Filter.Eq(v => v.Id, order.VendorId))
                    .FirstOrDefaultAsync();

                var vendorName = vendor?.Name ?? "Unknown Vendor";
                var expiryDate = order.CreditExpiryDate!.Value;
                var daysLeft = (int)Math.Ceiling((expiryDate - DateTime.UtcNow).TotalDays);
                var expiryText = expiryDate.ToString("dd MMM yyyy");

                string message;
                if (daysLeft <= 0)
                    message = $"Credit for {vendorName}'s order has expired ({expiryText}). Payment is overdue.";
                else if (daysLeft == 1)
                    message = $"Credit for {vendorName}'s order expires tomorrow ({expiryText}).";
                else
                    message = $"Credit for {vendorName}'s order expires in {daysLeft} days ({expiryText}).";

                await notificationService.CreateNotificationAsync(new CreateNotificationDto(
                    Type: "global",
                    UserId: null,
                    Message: message,
                    Metadata: new NotificationMetadataDto(
                        Link: $"/inventory/orders/{order.Id}",
                        Icon: "warning",
                        Category: "credit-expiry"
                    )
                ));

                await db.InventoryOrders.UpdateOneAsync(
                    Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, order.Id),
                    Builders<InventoryOrderModel>.Update.Set(o => o.CreditExpiryReminderSentAt, DateTime.UtcNow));
            }

            if (orders.Count > 0)
                _logger.LogInformation("Sent {Count} credit expiry reminder(s).", orders.Count);
        }
    }
}
