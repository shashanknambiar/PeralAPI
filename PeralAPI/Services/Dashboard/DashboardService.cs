using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;

namespace PeralAPI.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly MongoDbContext _db;

        public DashboardService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            var pendingOrdersTask = _db.InventoryOrders
                .CountDocumentsAsync(Builders<InventoryOrderModel>.Filter.Eq(o => o.Status, InventoryOrderStatus.Placed));

            var productsTask = _db.Products
                .Find(Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false))
                .ToListAsync();

            var quantityViewTask = _db.ProductQuantityView
                .Find(_ => true)
                .ToListAsync();

            var creditViewTask = _db.VendorCreditView
                .Find(_ => true)
                .ToListAsync();

            await Task.WhenAll(pendingOrdersTask, productsTask, quantityViewTask, creditViewTask);

            var products = productsTask.Result;
            var totalInventoryItems = products.Count;

            var quantityMap = quantityViewTask.Result.ToDictionary(q => q.ProductId, q => q.TotalQuantity);
            var lowStockItemsCount = products.Count(p =>
            {
                var qty = quantityMap.TryGetValue(p.Id, out var q) ? q : 0;
                return qty < p.MinQuantity;
            });

            var creditEntries = creditViewTask.Result;
            var pendingPaymentsCount = creditEntries.Count(c => c.Credit > 0);
            var totalPayableCredit = creditEntries.Where(c => c.Credit > 0).Sum(c => (double)c.Credit);
            var vendorsWithAdvanceCount = creditEntries.Count(c => c.Credit < 0);
            var totalAdvanceCredit = creditEntries.Where(c => c.Credit < 0).Sum(c => (double)c.Credit);

            return new DashboardDto(
                (int)pendingOrdersTask.Result,
                lowStockItemsCount,
                totalInventoryItems,
                pendingPaymentsCount,
                totalPayableCredit,
                vendorsWithAdvanceCount,
                totalAdvanceCredit
            );
        }
    }
}
