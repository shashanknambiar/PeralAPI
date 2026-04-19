using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PeralAPI.Models.Inventory
{
    public class VendorModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("contacts")]
        public List<ContactModel> Contacts { get; set; } = new();
    }

    public class ContactModel
    {
        [BsonElement("id")]
        public string Id { get; set; } = null!;
        [BsonElement("name")]
        public string Name { get; set; } = null!;
        [BsonElement("contact")]
        public string Contact { get; set; } = null!;
    }
}
