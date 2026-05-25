using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Models.Inventory
{
    public enum InventoryOrderStatus
    {
        Draft = 0,
        Placed = 1,
        Completed = 2,
        Cancelled = 3,
        RolledBack = 4,
        Received = 5
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
        [BsonElement("creditExpiryDate")]
        public DateTime? CreditExpiryDate { get; set; }
        [BsonElement("creditExpiryReminderSentAt")]
        public DateTime? CreditExpiryReminderSentAt { get; set; }
    }

    public class PaymentInformationModel
    {
        [BsonElement("value")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Value { get; set; }
        [BsonElement("amountPaid")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal AmountPaid { get; set; }
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
        public double Quantity { get; set; }
        [BsonElement("purchaseValue")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal PricePerItem { get; set; }
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

    public static class InventoryOrderModelExtensions
    {
        public static InventoryOrderDto ToDto(this InventoryOrderModel model, VendorDto Vendor, List<PurchaseItemDto> Products)
        {
            return new InventoryOrderDto(
                model.Id,
                Vendor,
                Products,
                model.Actions.Select(s => s.ToDto()).ToList(),
                model.Status,
                model.PaymentInformation.ToDto(),
                model.OrderCreatedOn,
                model.OrderClosedOn,
                model.CreditExpiryDate);
        }

        public static PaymentInformationDto ToDto(this PaymentInformationModel model)
        {
            return new PaymentInformationDto(
                model.Value,
                model.AmountPaid,
                model.PaymentDate,
                model.AccountNumber,
                model.PaymentMethod,
                model.ReferenceNumber,
                model.Attachment
            );
        }

        public static ActionDto ToDto(this ActionModel model)
        {
            return new ActionDto(model.ActionType, model.TimeStamp, model.PerformedBy, model.Remarks);
        }
    }
}
