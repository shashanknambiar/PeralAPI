using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Inventory
{
    public class PurchaseOrderModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("productId")]
        public List<PurchaseItmemModel> Products { get; set; } = new();
        [BsonElement("vendorId")]
        public string VendorId { get; set; } = null!;
        [BsonElement("orderDate")]
        public DateTime OrderDate { get; set; }
        [BsonElement("purchaseValue")]
        public int PurchaseValue { get; set; }
        [BsonElement("amountPaid")]
        public int AmountPaid { get; set; }
    }

    public class PurchaseItmemModel
    {
        [BsonElement("productId")]
        public string ProductId { get; set; } = null!;
        [BsonElement("quantity")]
        public int Quantity { get; set; }
        [BsonElement("purchaseValue")]
        public int PurchaseValue { get; set; }
    }
}
