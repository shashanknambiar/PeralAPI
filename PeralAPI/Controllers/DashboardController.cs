using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Services.Dashboard;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Tags("Dashboard")]
    [Authorize(Roles = "Admin,Inventory Manager")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboard;

        public DashboardController(IDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            var dto = await _dashboard.GetDashboardAsync();
            return Ok(dto);
        }
    }
}
