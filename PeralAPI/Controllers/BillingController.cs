using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/billing")]
    [Authorize(Roles = "Billing")]
    public class BillingController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public BillingController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        /// <summary>
        /// Searches Products by partial, case-insensitive name.
        /// Returns product ID, name, and current available quantity for each match.
        /// </summary>
        [HttpGet("products/search")]
        public async Task<IActionResult> SearchProduct([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { error = "Query parameter 'name' is required." });

            var results = await _inventory.GetProductAvailabilityAsync(name);
            return Ok(results);
        }

        /// <summary>
        /// Completes a bill by deducting the requested quantities from stock.
        /// Validates all items first — if any item has insufficient stock the entire
        /// bill is rejected with a 400 listing each shortfall.
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteBill([FromBody] List<BillItemDto> items)
        {
            if (items is null || items.Count == 0)
                return BadRequest(new { error = "Bill must contain at least one item." });

            var (success, failures) = await _inventory.CompleteBillAsync(items);

            if (!success)
            {
                var details = failures.Select(f => new
                {
                    productId = f.ProductId,
                    name = f.Name,
                    available = f.Available,
                    requested = f.Requested
                });

                return BadRequest(new
                {
                    error = "Insufficient stock for one or more items.",
                    items = details
                });
            }

            return Ok(new { message = "Bill completed successfully." });
        }
    }
}
