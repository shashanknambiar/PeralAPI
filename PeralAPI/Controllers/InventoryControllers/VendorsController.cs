using Microsoft.AspNetCore.Authorization;
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
            var vendorCredit = await _inventory.GetVendorCreditByIdAsync(new List<string> { created.Id });
            vendorCredit.TryGetValue(created.Id, out var credit);
            return CreatedAtAction(nameof(CreateVendor), new { id = created.Id }, created.ToDto(credit));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorDto>?>> GetAllVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vendors = await _inventory.GetVendorsAsync(page, pageSize);
            var vendorCredit = await _inventory.GetVendorCreditByIdAsync(vendors.Select(s=> s.Id).ToList());
            
            return Ok(vendors.Select(v =>
            {
                vendorCredit.TryGetValue(v.Id, out var credit);
                return v.ToDto(credit);
            }).ToList());
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VendorDto>?>> SearchVendors([FromQuery] string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vendors = await _inventory.SearchVendorsAsync(searchString, page, pageSize);
            var vendorCredit = await _inventory.GetVendorCreditByIdAsync(vendors.Select(s => s.Id).ToList());

            return Ok(vendors.Select(v =>
            {
                vendorCredit.TryGetValue(v.Id, out var credit);
                return v.ToDto(credit);
            }).ToList());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<VendorDto?>> GetVendorById(string id)
        {
            var vendor = await _inventory.GetVendorByIdAsync(id);

            if (vendor is null)
                return NotFound();
            var vendorCredit = await _inventory.GetVendorCreditByIdAsync(new List<string> { vendor.Id });
            vendorCredit.TryGetValue(vendor.Id, out var credit);
            return Ok(vendor.ToDto(credit));
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<VendorDto?>> UpdateVendor(string id, [FromBody] UpdateVendorDto dto)
        {
            var existing = await _inventory.GetVendorByIdAsync(id);
            if (existing?.IsReserved == true)
                return Conflict("Reserved vendors cannot be modified.");

            var updated = await _inventory.UpdateVendorAsync(id, dto);

            if (updated is null)
                return NotFound();
            var vendorCredit = await _inventory.GetVendorCreditByIdAsync(new List<string> { updated.Id });
            vendorCredit.TryGetValue(updated.Id, out var credit);
            return Ok(updated.ToDto(credit));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> DeleteVendor(string id)
        {
            var existing = await _inventory.GetVendorByIdAsync(id);
            if (existing?.IsReserved == true)
                return Conflict("Reserved vendors cannot be deleted.");

            var creditDict = await _inventory.GetVendorCreditByIdAsync(new List<string> { id });
            creditDict.TryGetValue(id, out var credit);
            if (credit != 0)
                return Conflict("Vendor account is not settled.");

            var deleted = await _inventory.DeleteVendorAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
