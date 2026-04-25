using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PeralAPI.Models.DTOs;
using System.Xml.Linq;

namespace PeralAPI.Models.Inventory
{

    public class ProductModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("name")]
        public string Name { get; set; } = null!;

        [BsonElement("vendorIds")]
        public List<string> VendorIds { get; set; } = new();

        [BsonElement("minQuantity")]
        public int MinQuantity { get; set; }

        [BsonElement("identifier")]
        public string? Identifier { get; set; }

        [BsonElement("imageUrl")]
        public string? ImageUrl { get; set; }
        [BsonElement("sellingPrice")]
        public int SellingPrice { get; set; }
        [BsonElement("isDeleted")]
        public bool IsDeleted { get; set; }
    }

    public class ProductQuantityViewModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ProductId { get; set; } = null!;
        [BsonElement("totalQuantity")]
        public int TotalQuantity { get; set; }
    }

    public static class ProductModelExtensions
    {
        public static ProductModel ToProductModel(this CreateProductDto dto)
        {
            return new ProductModel
            {
                Name = dto.Name,
                VendorIds = dto.VendorIds,
                MinQuantity = dto.MinQuantity,
                Identifier = dto.Identifier,
                ImageUrl = dto.ImageUrl,
                SellingPrice = dto.SellingPrice
            };
        }
        public static ProductModel ToProductModel(this UpdateProductDto dto)
        {
            return new ProductModel
            {
                Id = dto.Id,
                Name = dto.Name,
                VendorIds = dto.VendorIds,
                MinQuantity = dto.MinQuantity,
                Identifier = dto.Identifier,
                ImageUrl = dto.ImageUrl,
                SellingPrice = dto.SellingPrice
            };
        }
        public static ProductModel ToProductModel(this ProductDto dto)
        {
            return new ProductModel
            {
                Id = dto.Id,
                Name = dto.Name,
                VendorIds = dto.Vendors.Select(s=> s.Id).ToList(),
                MinQuantity = dto.MinQuantity,
                Identifier = dto.Identifier,
                ImageUrl = dto.ImageUrl,
                SellingPrice = dto.SellingPrice
            };
        }
        public static ProductSummaryDto ToProductSummaryDto(this ProductModel model)
        {
            return new ProductSummaryDto(
                model.Id,
                model.Name
            );
        }
        public static ProductDto ToDto(this ProductModel model, List<VendorModel> vendorModels, int quantityInInventory)
        {
            return new ProductDto(
                model.Id,
               model.Name,
                vendorModels.Where(v => model.VendorIds.Contains(v.Id))
                .Select(v =>
                new VendorNameDto(v.Id, v.Name))
                .ToList(),
                quantityInInventory,
                model.MinQuantity,
                model.Identifier,
                model.ImageUrl,
                model.SellingPrice
            );
        }
    }
}
