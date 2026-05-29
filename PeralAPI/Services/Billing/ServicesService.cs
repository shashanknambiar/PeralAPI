using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.Billing;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Services.Billing
{
    public class ServicesService : IServicesService
    {
        private readonly MongoDbContext _db;

        public ServicesService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<List<ServiceModel>> GetAllServicesAsync()
        {
            return await _db.Services
                .Find(s => !s.IsDeleted)
                .SortBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Dictionary<string, string>> GetServiceNamesByIdsAsync(IEnumerable<string> ids)
        {
            var idList = ids.ToList();
            if (idList.Count == 0) return new Dictionary<string, string>();

            var services = await _db.Services
                .Find(s => idList.Contains(s.Id))
                .ToListAsync();

            return services.ToDictionary(s => s.Id, s => s.Name);
        }

        public async Task<ServiceModel> CreateServiceAsync(CreateServiceDto dto)
        {
            var model = new ServiceModel
            {
                Name = dto.Name.Trim(),
                Price = dto.Price,
            };
            await _db.Services.InsertOneAsync(model);
            return model;
        }

        public async Task<ServiceModel?> UpdateServiceAsync(string id, UpdateServiceDto dto)
        {
            var filter = Builders<ServiceModel>.Filter.Eq(s => s.Id, id);
            var update = Builders<ServiceModel>.Update
                .Set(s => s.Name, dto.Name.Trim())
                .Set(s => s.Price, dto.Price);

            return await _db.Services.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<ServiceModel> { ReturnDocument = ReturnDocument.After }
            );
        }

        public async Task<bool> DeleteServiceAsync(string id)
        {
            var filter = Builders<ServiceModel>.Filter.Eq(s => s.Id, id);
            var update = Builders<ServiceModel>.Update.Set(s => s.IsDeleted, true);
            var result = await _db.Services.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }
    }
}
