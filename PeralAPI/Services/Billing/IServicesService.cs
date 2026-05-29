using PeralAPI.Models.Billing;
using PeralAPI.Models.DTOs;

namespace PeralAPI.Services.Billing
{
    public interface IServicesService
    {
        Task<List<ServiceModel>> GetAllServicesAsync();
        Task<Dictionary<string, string>> GetServiceNamesByIdsAsync(IEnumerable<string> ids);
        Task<ServiceModel> CreateServiceAsync(CreateServiceDto dto);
        Task<ServiceModel?> UpdateServiceAsync(string id, UpdateServiceDto dto);
        Task<bool> DeleteServiceAsync(string id);
    }
}
