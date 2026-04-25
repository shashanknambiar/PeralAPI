using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Inventory
{
    public enum InventoryOrderStatus
    {
        Placed,
        Completed,
        Cancelled,
        RolledBack
    }
    public class InventoryOrderModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        [BsonElement("products")]
        public List<PurchaseItemModel> Products { get; set; } = new();
        [BsonElement("vendorId")]
        public string VendorId { get; set; } = null!;
        [BsonElement("status")]
        public InventoryOrderStatus Status { get; set; }
        [BsonElement("actions")]
        public List<ActionModel> Actions { get; set; } = new();
        [BsonElement("paymentInformation")]
        public PaymentInformationModel PaymentInformation { get; set; } = null!;
        [BsonElement("OrderCreatedOn")]
        public DateTime OrderCreatedOn { get; set; }
        [BsonElement("OrderClosedOn")]
        public DateTime OrderClosedOn { get; set; } = DateTime.MinValue;
    }

    public class PaymentInformationModel
    {
        [BsonElement("value")]
        public int Value { get; set; }
        [BsonElement("amountPaid")]
        public int AmountPaid { get; set; }
        [BsonElement("paymentMethod")]
        public string PaymentMethod { get; set; } = null!;
        [BsonElement("paymentDate")]
        public DateTime PaymentDate { get; set; }
        [BsonElement("accountNumber")]
        public string AccountNumber { get; set; } = null!;
        [BsonElement("referenceNumber")]
        public string ReferenceNumber { get; set; } = null!;
        [BsonElement("attachement")]
        public byte[]? Attachment { get; set; }


    }

    public class PurchaseItemModel
    {
        [BsonElement("productId")]
        public string ProductId { get; set; } = null!;
        [BsonElement("quantity")]
        public int Quantity { get; set; }
        [BsonElement("purchaseValue")]
        public int PricePerItem { get; set; }
    }

    public class ActionModel
    {
        [BsonElement("actionType")]
        public string ActionType { get; set; } = null!;
        [BsonElement("timeStamp")]
        public DateTime TimeStamp { get; set; }
        [BsonElement("performedBy")]
        public string PerformedBy { get; set; } = null!;
        [BsonElement("remarks")]
        public string Remarks { get; set; } = null!;
    }
}
