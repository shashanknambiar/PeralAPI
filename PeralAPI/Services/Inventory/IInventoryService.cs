using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;

namespace PeralAPI.Services.Inventory
{
    public interface IInventoryService
    {

        #region Vendor Services

        Task<VendorModel> CreateVendorAsync(CreateVendorDto dto);
        Task<VendorModel?> UpdateVendorAsync(string id, UpdateVendorDto dto);
        Task<bool> DeleteVendorAsync(string id);
        Task<List<VendorModel>> GetVendorsAsync(int page, int pageSize);
        Task<VendorModel?> GetVendorByIdAsync(string id);
        Task<VendorModel?> GetAllVendorByIdAsync(string id);
        Task<List<VendorModel>> GetVendorByIdAsync(List<string> ids);
        Task<List<VendorModel>> GetAllVendorByIdAsync(List<string> ids);
        Task<List<VendorModel>> SearchVendorsAsync(string query, int page, int pageSize);

        #endregion

        #region Product Services
        Task<ProductModel> CreateProductAsync(CreateProductDto dto);
        Task<ProductModel?> UpdateProductAsync(string id, UpdateProductDto dto);
        Task<bool> DeleteProductAsync(string id);
        Task<List<ProductModel>> SearchProductAsync(string query, int page, int pageSize);
        Task<ProductModel?> GetProductByIdAsync(string id);
        Task<List<ProductModel>> GetAllProductByIdsAsync(List<string> ids);
        Task<List<ProductModel>> GetProductByIdsAsync(List<string> ids);
        #endregion

        #region Order Services
        Task<InventoryOrderModel> CreateInventoryOrderAsync(CreateInventoryOrderDto dto);
        Task<InventoryOrderModel> ChangeInventoryOrderStatus(ChangeInventoryOrderStatusDto dto);
        Task<InventoryOrderModel> UpdateInventoryOrderAsync(UpdateInventoryOrderDto dto);
        Task<List<InventoryOrderModel>> SearchInventoryOrdersAsync(string query, int page, int pageSize);
        Task<InventoryOrderModel?> GetInventoryOrderByIdAsync(string id);
        #endregion

        #region Product Transaction Ledger Services
        Task AddTransactionEntryAsync(ProductTransactionLedgerModel entry);
        Task AddTransactionEntriesAsync(List<ProductTransactionLedgerModel> entries);
        Task<List<ProductTransactionLedgerModel>> GetTransactionsByProductIdAsync(string productId, int page, int pageSize);
        Task<List<ProductTransactionLedgerModel>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate, int page, int pageSize);
        #endregion
    }
}
