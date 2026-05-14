using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers.InventoryControllers
{
    [Route("api/inventory/manager")]
    [Tags("Inventory Manager")]
    [Authorize(Roles = "Inventory Manager")]
    public class InventoryManagerController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public InventoryManagerController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        [HttpGet("stock-cards")]
        public async Task<ActionResult<List<StockCardDto>>> GetStockCards()
        {
            var cards = await _inventory.GetAllStockCardsAsync();
            return Ok(cards);
        }

        [HttpPost("confirm")]
        public async Task<ActionResult<ConfirmStockAdjustmentResultDto>> ConfirmStockAdjustment(
            [FromBody] ConfirmStockAdjustmentDto dto)
        {
            if (dto.Adjustments == null || dto.Adjustments.Count == 0)
                return BadRequest("No adjustments provided.");

            try
            {
                var result = await _inventory.ConfirmStockAdjustmentAsync(dto);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An internal error occurred.");
            }
        }
    }
}
