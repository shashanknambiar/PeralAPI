using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Inventory
{

    public class ProductModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("vendors")]
        public List<VendorModel> Vendors { get; set; } = new();

        [BsonElement("quantity")]
        public int Quantity { get; set; }

        [BsonElement("minQuantity")]
        public int MinQuantity { get; set; }

        [BsonElement("identifier")]
        public string? Identifier { get; set; }

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }
        [BsonElement("sellingPrice")]
        public int SellingPrice { get; set; }
    }
}
