using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Billing
{
    public class ServiceModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }
    }
}
