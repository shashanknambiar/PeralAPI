using PeralAPI.Models.Inventory;

namespace PeralAPI.Models.DTOs
{
    public class BillItemDto
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public record ProductAvailabilityDto(string ProductId, string Name, int AvailableQuantity);

    public record InsufficientStockDto(string ProductId, string Name, int Available, int Requested);

    /// <summary>Vendor payload for create/update — ID is generated server-side.</summary>
    public class CreateVendorDto
    {
        public string Name { get; set; } = null!;
        public List<ContactModelDto> Contacts { get; set; } = new();
    }

    public class ContactModelDto
    {
        public string Name { get; set; } = null!;
        public string Contact { get; set; } = null!;
    }

    /// <summary>Payload for creating a new product. Product ID and vendor IDs are generated server-side.</summary>
    public class CreateProductDto
    {
        public string Name { get; set; } = null!;
        public List<CreateVendorDto> Vendors { get; set; } = new();
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public string? Identifier { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>Payload for updating an existing product. Product ID comes from the route; vendor IDs are regenerated server-side.</summary>
    public class UpdateProductDto
    {
        public string Name { get; set; } = null!;
        public List<CreateVendorDto> Vendors { get; set; } = new();
        public int Quantity { get; set; }
        public int MinQuantity { get; set; }
        public string? Identifier { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>A single line item within a purchase order.</summary>
    public class PurchaseItemDto
    {
        public string ProductId { get; set; } = null!;
        public int Quantity { get; set; }
        public int PurchaseValue { get; set; }
    }

    /// <summary>Payload for creating a purchase order. Order ID is generated server-side. TotalPurchaseValue is computed server-side as the sum of item PurchaseValues.</summary>
    public class CreatePurchaseOrderDto
    {
        public string VendorId { get; set; } = null!;
        public List<PurchaseItemDto> Items { get; set; } = new();
        public DateTime OrderDate { get; set; }
        public int PurchaseValue { get; set; }
        public int AmountPaid { get; set; }
    }

    /// <summary>Payload for updating a purchase order. Order ID comes from the route.</summary>
    public class UpdatePurchaseOrderDto
    {
        public string VendorId { get; set; } = null!;
        public List<PurchaseItemDto> Items { get; set; } = new();
        public DateTime OrderDate { get; set; }
        public int AmountPaid { get; set; }
    }

    /// <summary>Purchase order response shape.</summary>
    public record PurchaseOrderDto(
        string Id,
        string VendorId,
        List<PurchaseItemDto> Items,
        DateTime OrderDate,
        int TotalPurchaseValue,
        int AmountPaid
    );
}
