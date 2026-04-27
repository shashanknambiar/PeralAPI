using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers.InventoryControllers
{
    [Route("api/inventory/vendors")]
    [Tags("Vendors")]
    [Authorize]
    public class VendorsController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public VendorsController(IInventoryService inventory)
        {
            _inventory = inventory;
        }
        [HttpPost]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<VendorDto>> CreateVendor([FromBody] CreateVendorDto dto)
        {
            var created = await _inventory.CreateVendorAsync(dto);
            return CreatedAtAction(nameof(CreateVendor), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorDto>?>> GetAllVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vendors = await _inventory.GetVendorsAsync(page, pageSize);
            return Ok(vendors.Select(v => v.ToDto()));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VendorDto>?>> SearchVendors([FromQuery] string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var results = await _inventory.SearchVendorsAsync(searchString, page, pageSize);
            return Ok(results.Select(v => v.ToDto()));
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorDto?>> GetVendorById(string id)
        {
            var vendor = await _inventory.GetVendorByIdAsync(id);

            if (vendor is null)
                return NotFound();

            return Ok(vendor.ToDto());
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<VendorDto?>> UpdateVendor(string id, [FromBody] UpdateVendorDto dto)
        {
            var updated = await _inventory.UpdateVendorAsync(id, dto);

            if (updated is null)
                return NotFound();
#warning fetch credit here.
            return Ok(updated.ToDto());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteVendor(string id)
        {
            var deleted = await _inventory.DeleteVendorAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
