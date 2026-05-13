using PeralAPI.Models.Billing;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Services.Billing
{
    public interface IBillingService
    {
        Task<BillingModel> CreateBillAsync(CreateBillDto dto);
        Task<BillingModel?> UpdateBillAsync(UpdateBillDto dto);
        Task<bool> DeleteBillAsync(string id);
        Task<(List<BillingModel> Bills, long TotalCount)> SearchBillsAsync(BillSearchParamsDto searchParams, int page, int pageSize);
        Task<BillingModel?> GetBillByIdAsync(string id);
    }
}
