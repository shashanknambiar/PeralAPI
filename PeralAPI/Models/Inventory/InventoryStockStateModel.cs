using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Inventory
{
    public class InventoryStockStateModel
    {
        [BsonId]
        public string ProductId { get; set; } = null!;

        [BsonElement("fillPercentage")]
        public int FillPercentage { get; set; } = 100;

        [BsonElement("floorPercentage")]
        public int FloorPercentage { get; set; } = 0;

        [BsonElement("lastCheckInOn")]
        public DateTime? LastCheckInOn { get; set; }
    }
}
