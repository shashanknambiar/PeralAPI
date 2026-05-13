using MongoDB.Bson;
using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;
using PeralAPI.Services;

namespace PeralAPI.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly MongoDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public InventoryService(MongoDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        /*
         * Vendor Services:
         * CreateVendorAsync
         * UpdateVendorAsync
         * DeleteVendorAsync: Add check for existing orders linked to the vendor to prevent orphaned references.
         * GetAllVendorsAsync
         * GetVendorByIdAsync
         * GetVendorByIdAsync(List<string> ids)
         * SearchVendorAsync
         * GetVendorCreditByIdAsync
         */

        #region Vendor Services
        public async Task<VendorModel> CreateVendorAsync(CreateVendorDto dto)
        {
            var vendor = new VendorModel
            {
                Name = dto.Name,
                Contacts = dto.Contacts.Select(c => new ContactModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = c.Name,
                    Contact = c.Contact,
                }).ToList(),
            };

            await _db.Vendors.InsertOneAsync(vendor);
            return vendor;
        }

        public async Task<VendorModel?> UpdateVendorAsync(string id, UpdateVendorDto dto)
        {
            var vendor = new VendorModel
            {
                Id = id,
                Name = dto.Name,
                Contacts = dto.Contacts.Select(c => new ContactModel
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    Name = c.Name,
                    Contact = c.Contact,
                }).ToList(),
            };

            var result = await _db.Vendors.ReplaceOneAsync(
                Builders<VendorModel>.Filter.Eq(v => v.Id, id)
                & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false),
                vendor);

            return result.ModifiedCount > 0 ? vendor : null;
        }

        public async Task<bool> DeleteVendorAsync(string id)
        {

            var result = await _db.Vendors.UpdateOneAsync(
                Builders<VendorModel>.Filter.Eq(v => v.Id, id),
                Builders<VendorModel>.Update.Set(v => v.IsDeleted, true));

            return result.ModifiedCount > 0;
        }

        public async Task<List<VendorModel>> GetVendorsAsync(int page, int pageSize)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false)
                & Builders<VendorModel>.Filter.Ne(v => v.IsReserved, true))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<VendorModel?> GetVendorByIdAsync(string id)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.Eq(v => v.Id, id)
                & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false))
                .FirstOrDefaultAsync();
        }
        public async Task<VendorModel?> GetAllVendorByIdAsync(string id)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.Eq(v => v.Id, id))
                .FirstOrDefaultAsync();
        }
        public async Task<List<VendorModel>> GetVendorByIdAsync(List<string> ids)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.In(v => v.Id, ids) 
                & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false))
                .ToListAsync();
        }
        public async Task<List<VendorModel>> GetAllVendorByIdAsync(List<string> ids)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.In(v => v.Id, ids))
                .ToListAsync();
        }
        public async Task<List<VendorModel>> SearchVendorsAsync(string query, int page, int pageSize)
        {
            var regex = new BsonRegularExpression(query, "i");
            var filter = Builders<VendorModel>.Filter.Regex(v => v.Name, regex)
                            & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false)
                            & Builders<VendorModel>.Filter.Ne(v => v.IsReserved, true);

            return await _db.Vendors
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetVendorCreditByIdAsync(List<string> ids)
        {
            var creditData = await _db.VendorCreditView
                .Find(Builders<VendorCreditViewModel>.Filter.In(c => c.VendorId, ids))
                .ToListAsync();
            return creditData.ToDictionary(c => c.VendorId, c => c.Credit);
        }

        #endregion


        /*
         * Product Services:
         * CreateProductAsync
         * UpdateProductAsync
         * DeleteProductAsync
         * SearchProductsSummaryAsync
         * SearchProductsAsync
         * GetProductByIdAsync
         * GetProductByIdsAsync
         * GetProductStockByIdsAsync
         */

        #region Product Services

        public async Task<ProductModel> CreateProductAsync(CreateProductDto dto)
        {
            var product = dto.ToProductModel();
            await _db.Products.InsertOneAsync(product);
            return product;
        }
        public async Task<ProductModel?> UpdateProductAsync(string id, UpdateProductDto dto)
        {
            var product = dto.ToProductModel();

            var result = await _db.Products.ReplaceOneAsync(
                Builders<ProductModel>.Filter.Eq(p => p.Id, id) & Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false),
                product);

            return result.ModifiedCount > 0 ? product : null;
        }
        public async Task<bool> DeleteProductAsync(string id)
        {
            var result = await _db.Products.UpdateOneAsync(
                Builders<ProductModel>.Filter.Eq(p => p.Id, id),
                Builders<ProductModel>.Update.Set(s => s.IsDeleted, true));

            return result.ModifiedCount > 0;
        }
        public async Task<List<ProductModel>> SearchProductAsync(string query, int page, int pageSize)
        {
            FilterDefinition<ProductModel> filter;

            if (string.IsNullOrWhiteSpace(query))
            {
                filter = Builders<ProductModel>.Filter.Empty & Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false);
                page = 1;
            }
            else
            {
                var regex = new BsonRegularExpression(query, "i");
                filter = Builders<ProductModel>.Filter.Regex(p => p.Name, regex) & Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false);
            }

            return await _db.Products
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
        public async Task<ProductModel?> GetProductByIdAsync(string id)
        {
            return await _db.Products
                .Find(Builders<ProductModel>.Filter.Eq(p => p.Id, id) & Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false))
                .FirstOrDefaultAsync();
        }
        public async Task<List<ProductModel>> GetProductByIdsAsync(List<string> ids)
        {
            return await _db.Products
                .Find(Builders<ProductModel>.Filter.In(p => p.Id, ids) & Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false))
                .ToListAsync();
        }
        public async Task<List<ProductModel>> GetAllProductByIdsAsync(List<string> ids)
        {
            return await _db.Products
                .Find(Builders<ProductModel>.Filter.In(p => p.Id, ids))
                .ToListAsync();
        }
        public async Task<Dictionary<string, double>> GetProductStockByIdsAsync(List<string> ids)
        {
            var stockData = await _db.ProductQuantityView
                .Find(Builders<ProductQuantityViewModel>.Filter.In(s => s.ProductId, ids))
                .ToListAsync();
            return stockData.ToDictionary(s => s.ProductId, s => s.TotalQuantity);
        }

        public async Task<Dictionary<string, string>> GetPlacedOrderIdsByProductIdsAsync(List<string> productIds)
        {
            var placedOrders = await _db.InventoryOrders
                .Find(Builders<InventoryOrderModel>.Filter.Eq(o => o.Status, InventoryOrderStatus.Placed))
                .SortByDescending(o => o.OrderCreatedOn)
                .ToListAsync();

            // Orders are sorted newest-first; first match per product is the latest order.
            var result = new Dictionary<string, string>();
            foreach (var order in placedOrders)
                foreach (var item in order.Products)
                    if (productIds.Contains(item.ProductId) && !result.ContainsKey(item.ProductId))
                        result[item.ProductId] = order.Id;

            return result;
        }
        #endregion

        /*
         * Order services:
         * CreateInventoryOrderAsync
         * ChangeOrderStatusAsync
         * UpdateInventoryOrderAsync
         * SearchOrdersAsync
         * GetOrderByIdAsync
         * 
         * CreateBillOrderAsync
         * CreateReturnBillOrderAsync
         *
         */

        #region Order Services

        public async Task<InventoryOrderModel> CreateInventoryOrderAsync(CreateInventoryOrderDto dto)
        {
            var order = new InventoryOrderModel
            {
                VendorId = dto.VendorId,
                Products = dto.ProductInfos.Select(i => new PurchaseItemModel
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    PricePerItem = i.PricePerItem
                }).ToList(),
                Status =  dto.IsPlaced ? InventoryOrderStatus.Placed  : InventoryOrderStatus.Draft,
                OrderCreatedOn = DateTime.Now,
                OrderClosedOn = DateTime.MinValue,
                PaymentInformation = new PaymentInformationModel
                {
                    Value = dto.PaymentInformation?.Value ?? 0,
                    AmountPaid = dto.PaymentInformation?.AmountPaid ?? 0,
                    PaymentDate = dto.PaymentInformation?.PaymentDate ?? DateTime.MinValue,
                    AccountNumber = dto.PaymentInformation?.AccountNumber ?? string.Empty,
                    PaymentMethod = dto.PaymentInformation?.PaymentMethod ?? string.Empty,
                    ReferenceNumber = dto.PaymentInformation?.ReferenceNumber ?? string.Empty,
                    Attachment = dto.PaymentInformation?.Attachment
                },
                Actions = new List<ActionModel>
                {
                    new ActionModel
                    {
                        ActionType = "Create",
                        TimeStamp = DateTime.UtcNow,
                        PerformedBy = _currentUser.UserName,
                        Remarks = dto.Remarks
                    }
                }
            };
            await _db.InventoryOrders.InsertOneAsync(order);
            return order;

        }
        public async Task<InventoryOrderModel> ChangeInventoryOrderStatus(ChangeInventoryOrderStatusDto dto)
        {
            var order = await _db.InventoryOrders.Find(Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, dto.Id)).FirstOrDefaultAsync();
            if (order == null)
                throw new Exception("Order not found.");

            ValidateInventoryOrderStatus(order.Status);

            if (dto.Status == InventoryOrderStatus.RolledBack && order.Status != InventoryOrderStatus.Completed)
                throw new Exception("Only completed orders can be rolled back.");

            ActionModel newAction = new()
            {
                ActionType = $"Status change from {order.Status.ToString()} to {dto.Status.ToString()}",
                PerformedBy = _currentUser.UserName,
                TimeStamp = DateTime.UtcNow,
                Remarks = dto.Remarks
            };
            order.Actions.Add(newAction);

            var update = Builders<InventoryOrderModel>.Update.Set(o => o.Status, dto.Status).AddToSet(o => o.Actions, newAction);
            long modifiedCount = 0;
            if (dto.Status == InventoryOrderStatus.Completed || dto.Status == InventoryOrderStatus.Cancelled || dto.Status == InventoryOrderStatus.RolledBack)
            {
                update = update.Set(o => o.OrderClosedOn, DateTime.UtcNow);
            }
            if (dto.Status == InventoryOrderStatus.Completed)
            {
                modifiedCount = await ChangeOrderStatusToCompleted(order, update);
            }
            else if (dto.Status == InventoryOrderStatus.RolledBack)
            {
                modifiedCount = await ChangeOrderStatusToRolledBack(order, update);
            }
            else
            {
                var result = await _db.InventoryOrders.UpdateOneAsync(
                    Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, dto.Id),
                    update);
                modifiedCount = result.ModifiedCount;
            }
                
            if (modifiedCount > 0)
            {
                return await _db.InventoryOrders.Find(Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, dto.Id)).FirstOrDefaultAsync()!;
            }
            else
            {
                throw new Exception("Failed to update order status.");
            }
        }

        private async Task<long> ChangeOrderStatusToCompleted(InventoryOrderModel order, UpdateDefinition<InventoryOrderModel> update)
        {
           
            // Build ledger entries from order products
            var ledgerEntries = order.Products.Select(p => new ProductTransactionLedgerModel
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                OrderId = order.Id,
                OrderSource = "InventoryOrder",
                TransactionDate = DateTime.UtcNow
            }).ToList();

            // Start a session for the transaction
            using var session = await _db.Client.StartSessionAsync();

            try
            {
                session.StartTransaction();

                // 1. Insert ledger entries first (financial record is source of truth)
                await _db.ProductTransactionLedger.InsertManyAsync(session, ledgerEntries);

                // 2. Mark order as completed
                var result = await _db.InventoryOrders.UpdateOneAsync(
                    session,
                    Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, order.Id),
                    update);

                await session.CommitTransactionAsync();
                return result.ModifiedCount;
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }


        }

        private async Task<long> ChangeOrderStatusToRolledBack(InventoryOrderModel order, UpdateDefinition<InventoryOrderModel> update)
        {
            // Negative ledger entries to reverse the stock added when the order was completed
            var ledgerEntries = order.Products.Select(p => new ProductTransactionLedgerModel
            {
                ProductId = p.ProductId,
                Quantity = -p.Quantity,
                OrderId = order.Id,
                OrderSource = "InventoryOrder",
                TransactionDate = DateTime.UtcNow
            }).ToList();

            // Zero out payment fields
            update = update
                .Set(o => o.PaymentInformation.Value, (decimal)0)
                .Set(o => o.PaymentInformation.AmountPaid, (decimal)0);

            using var session = await _db.Client.StartSessionAsync();

            try
            {
                session.StartTransaction();

                await _db.ProductTransactionLedger.InsertManyAsync(session, ledgerEntries);

                var result = await _db.InventoryOrders.UpdateOneAsync(
                    session,
                    Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, order.Id),
                    update);

                await session.CommitTransactionAsync();
                return result.ModifiedCount;
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task<InventoryOrderModel> UpdateInventoryOrderAsync(UpdateInventoryOrderDto dto)
        {
            var order = await _db.InventoryOrders.Find(Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, dto.Id)).FirstOrDefaultAsync();

            if (order == null)
            {
                throw new Exception("Order not found.");
            }

            ValidateInventoryOrderStatus(order.Status);

            order.VendorId = dto.VendorId;
            order.Products = dto.ProductInfos.Select(i => new PurchaseItemModel
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                PricePerItem = i.PricePerItem
            }).ToList();
            order.Actions.Add(new ActionModel()
            {
                ActionType = "Update",
                TimeStamp = DateTime.UtcNow,
                PerformedBy = _currentUser.UserName
            });
            order.PaymentInformation = new PaymentInformationModel
            {
                Value = dto.PaymentInformation?.Value ?? 0,
                AmountPaid = dto.PaymentInformation?.AmountPaid ?? 0,
                PaymentDate = dto.PaymentInformation?.PaymentDate ?? DateTime.MinValue,
                AccountNumber = dto.PaymentInformation?.AccountNumber ?? string.Empty,
                PaymentMethod = dto.PaymentInformation?.PaymentMethod ?? string.Empty,
                ReferenceNumber = dto.PaymentInformation?.ReferenceNumber ?? string.Empty,
                Attachment = dto.PaymentInformation?.Attachment
            };
            var result = await _db.InventoryOrders.ReplaceOneAsync(
                Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, dto.Id),
                order);
            return result.ModifiedCount > 0 ? order : throw new Exception("Failed to update inventory order.");
        }

        public async Task<(List<InventoryOrderModel> Orders, long TotalCount)> SearchInventoryOrdersAsync(OrderSearchParamsDto searchParams, int page, int pageSize)
        {
            var filters = new List<FilterDefinition<InventoryOrderModel>>();

            if (!string.IsNullOrWhiteSpace(searchParams.OrderId))
            {
                var regex = new BsonRegularExpression(searchParams.OrderId, "i");
                filters.Add(Builders<InventoryOrderModel>.Filter.Regex(o => o.Id, regex));
            }

            if (!string.IsNullOrWhiteSpace(searchParams.VendorName))
            {
                var vendorRegex = new BsonRegularExpression(searchParams.VendorName, "i");
                var matchingVendorIds = await _db.Vendors
                    .Find(Builders<VendorModel>.Filter.Regex(v => v.Name, vendorRegex)
                        & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false))
                    .Project(v => v.Id)
                    .ToListAsync();

                if (matchingVendorIds.Count == 0)
                    return (new List<InventoryOrderModel>(), 0);

                filters.Add(Builders<InventoryOrderModel>.Filter.In(o => o.VendorId, matchingVendorIds));
            }

            if (searchParams.Status.HasValue)
            {
                filters.Add(Builders<InventoryOrderModel>.Filter.Eq(o => o.Status, searchParams.Status.Value));
            }

            if (searchParams.FromDate.HasValue)
            {
                filters.Add(Builders<InventoryOrderModel>.Filter.Gte(o => o.OrderCreatedOn, searchParams.FromDate.Value.ToUniversalTime()));
            }

            if (searchParams.ToDate.HasValue)
            {
                filters.Add(Builders<InventoryOrderModel>.Filter.Lte(o => o.OrderCreatedOn, searchParams.ToDate.Value.ToUniversalTime()));
            }

            var filter = filters.Count > 0
                ? Builders<InventoryOrderModel>.Filter.And(filters)
                : Builders<InventoryOrderModel>.Filter.Empty;

            var countTask = _db.InventoryOrders.CountDocumentsAsync(filter);
            var ordersTask = _db.InventoryOrders
                .Find(filter)
                .SortByDescending(o => o.OrderCreatedOn)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            await Task.WhenAll(countTask, ordersTask);
            return (ordersTask.Result, countTask.Result);
        }
        public async Task<InventoryOrderModel?> GetInventoryOrderByIdAsync(string id)
        {
            return await _db.InventoryOrders
                .Find(Builders<InventoryOrderModel>.Filter.Eq(p => p.Id, id))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteInventoryOrderAsync(string id)
        {
            var order = await GetInventoryOrderByIdAsync(id);
            if (order == null) return false;
            if (order.Status != InventoryOrderStatus.Draft)
                throw new InvalidOperationException("Only Draft orders can be deleted.");
            var result = await _db.InventoryOrders.DeleteOneAsync(
                Builders<InventoryOrderModel>.Filter.Eq(o => o.Id, id));
            return result.DeletedCount > 0;
        }

        private static void ValidateInventoryOrderStatus(InventoryOrderStatus status)
        {
            if (status == InventoryOrderStatus.Cancelled
               || status == InventoryOrderStatus.RolledBack)
                throw new Exception("Order is locked. The order status cannot be changed.");
        }
        #endregion

        #region Inventory Manager Services

        public async Task<List<StockCardDto>> GetAllStockCardsAsync()
        {
            var products = await _db.Products
                .Find(Builders<ProductModel>.Filter.Eq(p => p.IsDeleted, false))
                .SortBy(p => p.Name)
                .ToListAsync();

            if (products.Count == 0)
                return [];

            var productIds = products.Select(p => p.Id).ToList();
            var quantityMapTask = GetProductStockByIdsAsync(productIds);
            var lastAdjMapTask = GetLastStockAdjustmentsByProductIdsAsync(productIds);
            var placedOrderMapTask = GetPlacedOrderIdsByProductIdsAsync(productIds);
            await Task.WhenAll(quantityMapTask, lastAdjMapTask, placedOrderMapTask);
            var quantityMap = quantityMapTask.Result;
            var lastAdjMap = lastAdjMapTask.Result;
            var placedOrderMap = placedOrderMapTask.Result;

            return products.Select(p =>
            {
                quantityMap.TryGetValue(p.Id, out var totalQty);
                lastAdjMap.TryGetValue(p.Id, out var lastAdj);
                placedOrderMap.TryGetValue(p.Id, out var placedOrderId);
                int fillPct = lastAdj?.FillPercentage ?? 100;

                return new StockCardDto(
                    p.Id,
                    p.Name,
                    p.Identifier,
                    p.MinQuantity,
                    p.ImageUrl,
                    totalQty,
                    fillPct,
                    lastAdj?.TransactionDate,
                    placedOrderId
                );
            }).ToList();
        }

        public async Task<ConfirmStockAdjustmentResultDto> ConfirmStockAdjustmentAsync(ConfirmStockAdjustmentDto dto)
        {
            var reservedVendor = await _db.Vendors
                .Find(v => v.Name == "Stock Adjustment" && v.IsReserved && !v.IsDeleted)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Stock Adjustment vendor not found. Please contact your administrator.");

            var productIds = dto.Adjustments.Select(a => a.ProductId).ToList();
            var lastAdjMap = await GetLastStockAdjustmentsByProductIdsAsync(productIds);

            var orderProducts = new List<PurchaseItemModel>();
            var ledgerEntries = new List<ProductTransactionLedgerModel>();
            double totalConsumed = 0;

            foreach (var item in dto.Adjustments)
            {
                lastAdjMap.TryGetValue(item.ProductId, out var lastAdj);
                int prevFill = lastAdj?.FillPercentage ?? 100;
                double delta = (item.NewFillPercentage - prevFill) / 100.0 - item.BoxesOpened;

                if (delta == 0) continue;

                if (delta < 0) totalConsumed += Math.Abs(delta);

                orderProducts.Add(new PurchaseItemModel { ProductId = item.ProductId, Quantity = delta, PricePerItem = 0 });
                ledgerEntries.Add(new ProductTransactionLedgerModel
                {
                    ProductId = item.ProductId,
                    Quantity = delta,
                    FillPercentage = item.NewFillPercentage,
                    OrderSource = "StockAdjustment",
                    TransactionDate = dto.CheckInDate
                });
            }

            if (ledgerEntries.Count == 0)
                return new ConfirmStockAdjustmentResultDto(0, 0, dto.CheckInDate);

            using var session = await _db.Client.StartSessionAsync();
            session.StartTransaction();
            try
            {
                var order = new InventoryOrderModel
                {
                    VendorId = reservedVendor.Id,
                    Products = orderProducts,
                    Status = InventoryOrderStatus.Completed,
                    OrderCreatedOn = dto.CheckInDate,
                    OrderClosedOn = dto.CheckInDate,
                    PaymentInformation = new PaymentInformationModel
                    {
                        Value = 0, AmountPaid = 0, PaymentMethod = string.Empty,
                        AccountNumber = string.Empty, ReferenceNumber = string.Empty,
                        PaymentDate = dto.CheckInDate
                    },
                    Actions = [new ActionModel { ActionType = "Stock Adjustment", TimeStamp = dto.CheckInDate, PerformedBy = _currentUser.UserName, Remarks = string.Empty }]
                };
                await _db.InventoryOrders.InsertOneAsync(session, order);

                foreach (var entry in ledgerEntries)
                    entry.OrderId = order.Id;

                await _db.ProductTransactionLedger.InsertManyAsync(session, ledgerEntries);
                await session.CommitTransactionAsync();
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }

            return new ConfirmStockAdjustmentResultDto(ledgerEntries.Count, Math.Round(totalConsumed, 2), dto.CheckInDate);
        }

        private async Task<Dictionary<string, ProductTransactionLedgerModel>> GetLastStockAdjustmentsByProductIdsAsync(List<string> productIds)
        {
            var entries = await _db.ProductTransactionLedger
                .Find(Builders<ProductTransactionLedgerModel>.Filter.In(e => e.ProductId, productIds)
                    & Builders<ProductTransactionLedgerModel>.Filter.Eq(e => e.OrderSource, "StockAdjustment"))
                .SortByDescending(e => e.TransactionDate)
                .ToListAsync();

            return entries
                .GroupBy(e => e.ProductId)
                .ToDictionary(g => g.Key, g => g.First());
        }

        #endregion

        /*
         * Product Transaction Ledger Services:
         * AddTransactionEntryAsync
         * AddTransactionEntriesAsync
         * GetTransactionsByProductIdAsync
         * GetTransactionsByDateRangeAsync
         * GetTransactionsAsync
         */

        #region Product Transaction Ledger Services
        public async Task AddTransactionEntryAsync(ProductTransactionLedgerModel entry)
        {
            await _db.ProductTransactionLedger.InsertOneAsync(entry);
        }

        public async Task AddTransactionEntriesAsync(List<ProductTransactionLedgerModel> entries)
        {
            await _db.ProductTransactionLedger.InsertManyAsync(entries);
        }

        public async Task<List<ProductTransactionLedgerModel>> GetTransactionsByProductIdAsync(string productId, int page, int pageSize)
        {
            return await _db.ProductTransactionLedger
                .Find(Builders<ProductTransactionLedgerModel>.Filter.Eq(e => e.ProductId, productId))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
        public async Task<List<ProductTransactionLedgerModel>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            return await _db.ProductTransactionLedger
                .Find(Builders<ProductTransactionLedgerModel>.Filter.Gte(e => e.TransactionDate, startDate) & Builders<ProductTransactionLedgerModel>.Filter.Lte(e => e.TransactionDate, endDate))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
        #endregion
    }
}
