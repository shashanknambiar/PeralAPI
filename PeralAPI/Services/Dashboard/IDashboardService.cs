using PeralAPI.Models.DTOs;

namespace PeralAPI.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync();
    }
}
