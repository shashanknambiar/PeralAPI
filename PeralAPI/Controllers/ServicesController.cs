using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Services.Billing;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/billing/services")]
    [Tags("BillingServices")]
    [Authorize(Roles = "Billing")]
    public class ServicesController : ControllerBase
    {
        private readonly IServicesService _services;

        public ServicesController(IServicesService services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceDto>>> GetAllServices()
        {
            var services = await _services.GetAllServicesAsync();
            return Ok(services.Select(s => new ServiceDto(s.Id, s.Name, s.Price)).ToList());
        }

        [HttpPost]
        public async Task<ActionResult<ServiceDto>> CreateService([FromBody] CreateServiceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Service name is required.");
            if (dto.Price < 0)
                return BadRequest("Price cannot be negative.");

            var created = await _services.CreateServiceAsync(dto);
            return CreatedAtAction(nameof(GetAllServices), new ServiceDto(created.Id, created.Name, created.Price));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceDto>> UpdateService(string id, [FromBody] UpdateServiceDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Service name is required.");
            if (dto.Price < 0)
                return BadRequest("Price cannot be negative.");

            var updated = await _services.UpdateServiceAsync(id, dto);
            if (updated == null)
                return NotFound("Service not found.");

            return Ok(new ServiceDto(updated.Id, updated.Name, updated.Price));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteService(string id)
        {
            var deleted = await _services.DeleteServiceAsync(id);
            if (!deleted)
                return NotFound("Service not found.");
            return NoContent();
        }
    }
}
