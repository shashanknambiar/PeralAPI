using MongoDB.Bson;
using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;
using StackExchange.Redis;

namespace PeralAPI.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly MongoDbContext _db;

        public InventoryService(MongoDbContext db)
        {
            _db = db;
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
                .Find(Builders<VendorModel>.Filter.Empty
                & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false))
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
                            & Builders<VendorModel>.Filter.Eq(v => v.IsDeleted, false);

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
        public async Task<Dictionary<string, int>> GetProductStockByIdsAsync(List<string> ids)
        {
            var stockData = await _db.ProductQuantityView
                .Find(Builders<ProductQuantityViewModel>.Filter.In(s => s.ProductId, ids))
                .ToListAsync();
            return stockData.ToDictionary(s => s.ProductId, s => s.TotalQuantity);
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
                        PerformedBy = "",
                        Remarks = dto.Remarks
                        #warning Get user information for audit trail
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

            ActionModel newAction = new()
            {
                ActionType = $"Status change from {order.Status.ToString()} to {dto.Status.ToString()}",
                PerformedBy = "",
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
            if(dto.Status == InventoryOrderStatus.Completed)
            {
                modifiedCount = await ChangeOrderStatusToCompleted(order, update);
            }
            else
            {
               var result =  await _db.InventoryOrders.UpdateOneAsync(
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
                PerformedBy = ""
#warning Get user information for audit trail
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
               || status == InventoryOrderStatus.RolledBack
               || status == InventoryOrderStatus.Completed)
                throw new Exception("Order is locked. The order status cannot be changed.");
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
