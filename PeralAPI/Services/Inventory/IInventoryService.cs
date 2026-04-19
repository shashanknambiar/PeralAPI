using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;

namespace PeralAPI.Services.Inventory
{
    public interface IInventoryService
    {
        /// <summary>
        /// Creates a new product from the DTO.
        /// A MongoDB ObjectId is generated for the product; a new ObjectId is generated for each vendor.
        /// Returns the persisted document.
        /// </summary>
        Task<ProductModel> CreateProductAsync(CreateProductDto dto);

        /// <summary>
        /// Replaces an existing product using the route <paramref name="id"/>.
        /// Vendor IDs are regenerated server-side.
        /// Returns the updated document or null if not found.
        /// </summary>
        Task<ProductModel?> UpdateProductAsync(string id, UpdateProductDto dto);

        /// <summary>Deletes a product by ID; returns true if a document was removed.</summary>
        Task<bool> DeleteProductAsync(string id);

        /// <summary>Returns a single product by ID, or null if not found.</summary>
        Task<ProductModel?> GetProductByIdAsync(string id);

        /// <summary>Case-insensitive partial-name search across Products, paginated.</summary>
        Task<List<ProductModel>> SearchProductsAsync(string query, int page, int pageSize);

        /// <summary>Returns all products in insertion order, paginated.</summary>
        Task<List<ProductModel>> GetAllProductsAsync(int page, int pageSize);

        /// <summary>
        /// Searches ProductQuantities by partial, case-insensitive name match.
        /// Returns each matching entry's ID, name, and current available quantity.
        /// </summary>
        Task<List<ProductAvailabilityDto>> GetProductAvailabilityAsync(string name);

        /// <summary>Creates a new standalone vendor. Returns the persisted document.</summary>
        Task<VendorModel> CreateVendorAsync(CreateVendorDto dto);

        /// <summary>Returns all vendors, paginated.</summary>
        Task<List<VendorModel>> GetAllVendorsAsync(int page, int pageSize);

        /// <summary>Case-insensitive partial-name search across Vendors, paginated.</summary>
        Task<List<VendorModel>> SearchVendorsAsync(string query, int page, int pageSize);

        /// <summary>Returns a single vendor by ID, or null if not found.</summary>
        Task<VendorModel?> GetVendorByIdAsync(string id);

        /// <summary>Replaces a vendor's name and contacts. Contact IDs are regenerated server-side. Returns the updated document or null if not found.</summary>
        Task<VendorModel?> UpdateVendorAsync(string id, CreateVendorDto dto);

        /// <summary>Deletes a vendor by ID; returns true if a document was removed.</summary>
        Task<bool> DeleteVendorAsync(string id);

        /// <summary>Creates a new purchase order. Order ID is generated server-side. TotalPurchaseValue and Quantity are computed as the sum of item values.</summary>
        Task<PurchaseOrderDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto);

        /// <summary>
        /// Validates that every item has sufficient stock before deducting any quantities.
        /// Returns (true, empty) on success or (false, failures) listing each shortfall.
        /// All-or-nothing in logic; note this is not a MongoDB multi-document transaction.
        /// </summary>
        Task<(bool Success, List<InsufficientStockDto> Failures)> CompleteBillAsync(List<BillItemDto> items);
    }
}
