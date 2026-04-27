using PeralAPI.Models.Inventory;

namespace PeralAPI.Models.DTOs
{
    /*
     * Vendor DTO
     */
    public record VendorDto(
        string Id,
        string Name,
        List<ContactDto> Contacts, 
        int Credit);

    public record CreateVendorDto(string Name,
        List<ContactDto> Contacts);

    public record UpdateVendorDto(string Id, string Name, List<ContactDto> Contacts) 
        : CreateVendorDto(Name, Contacts);

    public record ContactDto(
        string Id,
        string Name,
        string Contact);


    public record VendorNameDto
        (string Id,
        string Name);

    /*
     * Porduct DTOs
     */
    /// <summary>Product response with resolved vendor names joined from the vendors collection.</summary>
    /// Quantity is calculated server side based on the products ledger.
    /// Quantity should not be set by the client when creating or updating a product, as it is derived from the inventory transactions.</summary>

    public record ProductDto(
        string Id,
        string Name,
        List<VendorNameDto> Vendors,
        int Quantity,
        int MinQuantity,
        string? Identifier,
        string? ImageUrl,
        int SellingPrice);
    public record UpdateProductDto(
        string Id,
        string Name,
        List<string> VendorIds,
        int MinQuantity,
        string? Identifier,
        string? ImageUrl,
        int SellingPrice);
    public record CreateProductDto(
        string Name,
        List<string> VendorIds,
        int MinQuantity,
        string? Identifier,
        string? ImageUrl,
        int SellingPrice);
    public record ProductSummaryDto(string Id, string Name);

    /*
     * Order DTOs 
     */
    /// <summary>Payload for hadnleing a purchase order. Order ID is generated server-side. TotalPurchaseValue is computed server-side as the sum of item PurchaseValues.</summary>
    public record InventoryOrderDto(
        string Id,
        VendorDto Vendor,
        List<PurchaseItemDto> Products,
        List<ActionDto> Actions,
        InventoryOrderStatus Status,
        PaymentInformationDto PaymentInformation,
        DateTime OrderCreatedOn,
        DateTime OrderClosedOn
    );
    public record CreateInventoryOrderDto(
        string VendorId,
        List<ProductIdAndQuantity> ProductInfos,
        PaymentInformationDto PaymentInformation,
        string Remarks
    );
    public record UpdateInventoryOrderDto(string Id,
        string VendorId,
        List<ProductIdAndQuantity> ProductInfos,
        PaymentInformationDto PaymentInformation,
        string Remarks);
    public record ChangeInventoryOrderStatusDto(string Id, InventoryOrderStatus Status, string Remarks);
    public record ProductIdAndQuantity(string ProductId, int Quantity, int PricePerItem);
    public record PaymentInformationDto(
        int Value,  
        int AmountPaid, 
        DateTime PaymentDate, 
        string AccountNumber, 
        string PaymentMethod, 
        string ReferenceNumber, 
        byte[]? Attachment);
    public record PurchaseItemDto(string ProductId,
    string ProductName,
    int Quantity,
    int PricePerItem);

    public record ActionDto(string ActionType, DateTime ActionDate, string PerformedBy, string Remarks);

    /*
     *Prdoduct Transaction Ledger DTO
     *DTO to show entries in from the immutable product transaction ledger collection.
     *Only Read DTO. 
     *No Updates allowed, Creation is allowed only through inventory orders or billing orders.
     */

    public record ProductTransactionLedgerDto
        (
        string Id,
        string OrderSource,
        string OrderId,
        string ProductId,
        string ProductName,
        int Quantity);

}
