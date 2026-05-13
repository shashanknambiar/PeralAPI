using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Models.Inventory
{
    public class VendorModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        
        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; } = false;

        [BsonElement("isReserved")]
        public bool IsReserved { get; set; } = false;

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

    public class VendorCreditViewModel
    {
        [BsonElement("vendorId")]
        public string VendorId { get; set; } = null!;
        [BsonElement("credit")]
        public int Credit { get; set; }
    }

    public static class VendorModelExtensions
    {
        public static VendorDto ToDto(this VendorModel vendor, int credit = 0)
        {
            return new VendorDto(
                vendor.Id,
                vendor.Name,
                vendor.Contacts.Select(c => new ContactDto(c.Id, c.Name, c.Contact)).ToList(),
                credit
            );
        }

        public static VendorModel ToVendorModel(this VendorDto vendorDto)
        {
            return new VendorModel
            {
                Id = vendorDto.Id,
                Name = vendorDto.Name,
                Contacts = vendorDto.Contacts.Select(c => new ContactModel { Name = c.Name, Contact = c.Contact }).ToList()
            };
        }
    }

}
