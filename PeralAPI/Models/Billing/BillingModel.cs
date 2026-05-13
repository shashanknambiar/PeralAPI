using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Models.Billing
{
    public class BillingModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("patientName")]
        public string PatientName { get; set; } = null!;

        [BsonElement("billDate")]
        public DateTime BillDate { get; set; }

        [BsonElement("patientPhoneNumber")]
        public string PatientPhoneNumber { get; set; } = null!;

        [BsonElement("age")]
        public int Age { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; } = null!;

        [BsonElement("doctorName")]
        public string DoctorName { get; set; } = null!;

        [BsonElement("products")]
        public List<BillingProductItem> Products { get; set; } = new();

        [BsonElement("discountInPercent")]
        public double DiscountInPercent { get; set; }

        [BsonElement("billTotal")]
        public double BillTotal { get; set; }
    }

    public class BillingProductItem
    {
        [BsonElement("productId")]
        public string ProductId { get; set; } = null!;

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("pricePerItem")]
        public decimal PricePerItem { get; set; }
    }

    public static class BillingModelExtensions
    {
        public static BillingModel ToModel(this CreateBillDto dto) => new()
        {
            PatientName = dto.PatientName,
            BillDate = dto.BillDate,
            PatientPhoneNumber = dto.PatientPhoneNumber,
            Age = dto.Age,
            Gender = dto.Gender,
            DoctorName = dto.DoctorName,
            Products = dto.Products.Select(p => new BillingProductItem
            {
                ProductId = p.ProductId,
                Quantity = p.Quantity,
                PricePerItem = p.PricePerItem,
            }).ToList(),
            DiscountInPercent = dto.DiscountInPercent,
            BillTotal = dto.BillTotal,
        };

        public static BillDto ToDto(this BillingModel model, Dictionary<string, string> productNames) => new(
            model.Id,
            model.PatientName,
            model.BillDate,
            model.PatientPhoneNumber,
            model.Age,
            model.Gender,
            model.DoctorName,
            model.Products.Select(p => new BillProductItemDto(
                p.ProductId,
                productNames.TryGetValue(p.ProductId, out var name) ? name : p.ProductId,
                p.Quantity,
                p.PricePerItem
            )).ToList(),
            model.DiscountInPercent,
            model.BillTotal
        );
    }
}
