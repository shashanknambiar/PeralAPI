using MongoDB.Bson;
using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;

namespace PeralAPI.Services.Inventory
{
    public class InventoryService : IInventoryService
    {
        private readonly MongoDbContext _db;

        public InventoryService(MongoDbContext db)
        {
            _db = db;
        }

        private static ProductModel MapFromDto(CreateProductDto dto) => new()
        {
            Name = dto.Name,
            Quantity = dto.Quantity,
            MinQuantity = dto.MinQuantity,
            Identifier = dto.Identifier,
            ImageUrl = dto.ImageUrl,
            Vendors = dto.Vendors.Select(v => new VendorModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = v.Name,
                Contacts = new(),
            }).ToList(),
        };

        private static ProductModel MapFromDto(string id, UpdateProductDto dto) => new()
        {
            Id = id,
            Name = dto.Name,
            Quantity = dto.Quantity,
            MinQuantity = dto.MinQuantity,
            Identifier = dto.Identifier,
            ImageUrl = dto.ImageUrl,
            Vendors = dto.Vendors.Select(v => new VendorModel
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Name = v.Name,
                Contacts = new(),
            }).ToList(),
        };

        /// <inheritdoc/>
        public async Task<ProductModel> CreateProductAsync(CreateProductDto dto)
        {
            var product = MapFromDto(dto);
            await _db.Products.InsertOneAsync(product);
            return product;
        }

        /// <inheritdoc/>
        public async Task<ProductModel?> UpdateProductAsync(string id, UpdateProductDto dto)
        {
            var product = MapFromDto(id, dto);

            var result = await _db.Products.ReplaceOneAsync(
                Builders<ProductModel>.Filter.Eq(p => p.Id, id),
                product);

            return result.ModifiedCount > 0 ? product : null;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteProductAsync(string id)
        {
            var result = await _db.Products.DeleteOneAsync(
                Builders<ProductModel>.Filter.Eq(p => p.Id, id));

            return result.DeletedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<ProductModel?> GetProductByIdAsync(string id)
        {
            return await _db.Products
                .Find(Builders<ProductModel>.Filter.Eq(p => p.Id, id))
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ProductModel>> SearchProductsAsync(string query, int page, int pageSize)
        {
            var regex = new BsonRegularExpression(query, "i");
            var filter = Builders<ProductModel>.Filter.Regex(p => p.Name, regex);

            return await _db.Products
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ProductModel>> GetAllProductsAsync(int page, int pageSize)
        {
            return await _db.Products
                .Find(Builders<ProductModel>.Filter.Empty)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ProductAvailabilityDto>> GetProductAvailabilityAsync(string name)
        {
            var regex = new BsonRegularExpression(name, "i");
            var filter = Builders<ProductModel>.Filter.Regex(p => p.Name, regex);

            var results = await _db.Products.Find(filter).ToListAsync();

            return results
                .Select(r => new ProductAvailabilityDto(r.Id, r.Name, r.Quantity))
                .ToList();
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public async Task<List<VendorModel>> GetAllVendorsAsync(int page, int pageSize)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.Empty)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<VendorModel>> SearchVendorsAsync(string query, int page, int pageSize)
        {
            var regex = new BsonRegularExpression(query, "i");
            var filter = Builders<VendorModel>.Filter.Regex(v => v.Name, regex);

            return await _db.Vendors
                .Find(filter)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<VendorModel?> GetVendorByIdAsync(string id)
        {
            return await _db.Vendors
                .Find(Builders<VendorModel>.Filter.Eq(v => v.Id, id))
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<VendorModel?> UpdateVendorAsync(string id, CreateVendorDto dto)
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
                Builders<VendorModel>.Filter.Eq(v => v.Id, id),
                vendor);

            return result.ModifiedCount > 0 ? vendor : null;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteVendorAsync(string id)
        {
            var result = await _db.Vendors.DeleteOneAsync(
                Builders<VendorModel>.Filter.Eq(v => v.Id, id));

            return result.DeletedCount > 0;
        }

        /// <inheritdoc/>
        public async Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto)
        {
            var items = dto.Items.Select(i => new PurchaseItmemModel
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                PurchaseValue = i.PurchaseValue,
            }).ToList();

            var order = new PurchaseOrderModel
            {
                VendorId = dto.VendorId,
                PurchaseValue = dto.PurchaseValue,
                OrderDate = dto.OrderDate,
                AmountPaid = dto.AmountPaid,
                Products = items,
            };

            await _db.PurchaseOrders.InsertOneAsync(order);
            
            //Trigger Ledger update (not implemented yet)

            return new PurchaseOrderDto(
                order.Id,
                order.VendorId,
                dto.Items,
                order.OrderDate,
                order.PurchaseValue,
                order.AmountPaid
            );
        }

        /// <inheritdoc/>
        public async Task<(bool Success, List<InsufficientStockDto> Failures)> CompleteBillAsync(
            List<BillItemDto> items)
        {
            var productIds = items.Select(i => i.ProductId).ToList();
            var filter = Builders<ProductModel>.Filter.In(p => p.Id, productIds);
            var products = await _db.Products.Find(filter).ToListAsync();
            var stockMap = products.ToDictionary(p => p.Id);

            // Validate ALL items before deducting any (all-or-nothing in logic;
            // note: not a MongoDB multi-document transaction — concurrent requests could race)
            var failures = new List<InsufficientStockDto>();

            foreach (var item in items)
            {
                if (!stockMap.TryGetValue(item.ProductId, out var product))
                {
                    failures.Add(new InsufficientStockDto(item.ProductId, "Unknown", 0, item.Quantity));
                    continue;
                }

                if (product.Quantity < item.Quantity)
                    failures.Add(new InsufficientStockDto(item.ProductId, product.Name, product.Quantity, item.Quantity));
            }

            if (failures.Count > 0)
                return (false, failures);

            foreach (var item in items)
            {
                var updateFilter = Builders<ProductModel>.Filter.Eq(p => p.Id, item.ProductId);
                var update = Builders<ProductModel>.Update.Inc(p => p.Quantity, -item.Quantity);
                await _db.Products.UpdateOneAsync(updateFilter, update);
            }

            return (true, new List<InsufficientStockDto>());
        }
    }
}
