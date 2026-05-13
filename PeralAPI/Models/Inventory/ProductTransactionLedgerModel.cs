using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Inventory
{
    public class ProductTransactionLedgerModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("orderSource")]
        public string OrderSource { get; set; } = null!;
        [BsonElement("orderId")]
        public string OrderId { get; set; } = null!;
        [BsonElement("productId")]
        public string ProductId { get; set; } = null!;
        [BsonElement("quantity")]
        public double Quantity { get; set; }
        [BsonElement("fillPercentage")]
        public int? FillPercentage { get; set; }
        [BsonElement("transactionDate")]
        public DateTime TransactionDate { get; set; }
    }
}