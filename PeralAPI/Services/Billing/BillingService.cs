using MongoDB.Driver;
using PeralAPI.Database;
using PeralAPI.Models.Billing;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Services.Billing
{
    public class BillingService : IBillingService
    {
        private readonly MongoDbContext _db;

        public BillingService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<BillingModel> CreateBillAsync(CreateBillDto dto)
        {
            var model = dto.ToModel();
            await _db.Bills.InsertOneAsync(model);
            return model;
        }

        public async Task<BillingModel?> UpdateBillAsync(UpdateBillDto dto)
        {
            var filter = Builders<BillingModel>.Filter.Eq(b => b.Id, dto.Id);
            var update = Builders<BillingModel>.Update
                .Set(b => b.PatientName, dto.PatientName)
                .Set(b => b.BillDate, dto.BillDate)
                .Set(b => b.PatientPhoneNumber, dto.PatientPhoneNumber)
                .Set(b => b.Age, dto.Age)
                .Set(b => b.Gender, dto.Gender)
                .Set(b => b.DoctorName, dto.DoctorName)
                .Set(b => b.Products, dto.Products.Select(p => new BillingProductItem
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity,
                    PricePerItem = p.PricePerItem,
                }).ToList())
                .Set(b => b.DiscountInPercent, dto.DiscountInPercent)
                .Set(b => b.BillTotal, dto.BillTotal);

            var result = await _db.Bills.FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<BillingModel> { ReturnDocument = ReturnDocument.After }
            );
            return result;
        }

        public async Task<bool> DeleteBillAsync(string id)
        {
            var result = await _db.Bills.DeleteOneAsync(b => b.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<(List<BillingModel> Bills, long TotalCount)> SearchBillsAsync(
            BillSearchParamsDto searchParams, int page, int pageSize)
        {
            var builder = Builders<BillingModel>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrWhiteSpace(searchParams.PatientName))
                filter &= builder.Regex(b => b.PatientName,
                    new MongoDB.Bson.BsonRegularExpression(searchParams.PatientName, "i"));

            if (!string.IsNullOrWhiteSpace(searchParams.PatientPhoneNumber))
                filter &= builder.Regex(b => b.PatientPhoneNumber,
                    new MongoDB.Bson.BsonRegularExpression(searchParams.PatientPhoneNumber, "i"));

            if (!string.IsNullOrWhiteSpace(searchParams.DoctorName))
                filter &= builder.Regex(b => b.DoctorName,
                    new MongoDB.Bson.BsonRegularExpression(searchParams.DoctorName, "i"));

            if (searchParams.FromDate.HasValue)
                filter &= builder.Gte(b => b.BillDate, searchParams.FromDate.Value);

            if (searchParams.ToDate.HasValue)
                filter &= builder.Lte(b => b.BillDate, searchParams.ToDate.Value);

            var totalCount = await _db.Bills.CountDocumentsAsync(filter);
            var bills = await _db.Bills
                .Find(filter)
                .SortByDescending(b => b.BillDate)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (bills, totalCount);
        }

        public async Task<BillingModel?> GetBillByIdAsync(string id)
        {
            return await _db.Bills.Find(b => b.Id == id).FirstOrDefaultAsync();
        }
    }
}
