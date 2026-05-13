using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.Billing;
using PeralAPI.Models.DTOs;
using PeralAPI.Services.Billing;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/billing")]
    [Tags("Billing")]
    [Authorize(Roles = "Billing")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billing;
        private readonly IInventoryService _inventory;

        public BillingController(IBillingService billing, IInventoryService inventory)
        {
            _billing = billing;
            _inventory = inventory;
        }

        [HttpPost]
        public async Task<ActionResult<BillDto>> CreateBill([FromBody] CreateBillDto dto)
        {
            var bill = await _billing.CreateBillAsync(dto);
            var productNames = await ResolveProductNamesAsync(bill);
            return Ok(bill.ToDto(productNames));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BillDto>> UpdateBill(string id, [FromBody] UpdateBillDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");

            var bill = await _billing.UpdateBillAsync(dto);
            if (bill == null)
                return NotFound("Bill not found.");

            var productNames = await ResolveProductNamesAsync(bill);
            return Ok(bill.ToDto(productNames));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBill(string id)
        {
            var deleted = await _billing.DeleteBillAsync(id);
            if (!deleted)
                return NotFound("Bill not found.");
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BillDto>> GetBillById(string id)
        {
            var bill = await _billing.GetBillByIdAsync(id);
            if (bill == null)
                return NotFound("Bill not found.");

            var productNames = await ResolveProductNamesAsync(bill);
            return Ok(bill.ToDto(productNames));
        }

        [HttpGet("search")]
        public async Task<ActionResult<BillSearchResultDto>> SearchBills(
            [FromQuery] string? patientName = null,
            [FromQuery] string? patientPhoneNumber = null,
            [FromQuery] string? doctorName = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var searchParams = new BillSearchParamsDto(patientName, patientPhoneNumber, doctorName, fromDate, toDate);
            var (bills, totalCount) = await _billing.SearchBillsAsync(searchParams, page, pageSize);
            var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;

            var billDtos = new List<BillDto>();
            foreach (var bill in bills)
            {
                var productNames = await ResolveProductNamesAsync(bill);
                billDtos.Add(bill.ToDto(productNames));
            }

            return Ok(new BillSearchResultDto(page, totalPages, billDtos));
        }

        private async Task<Dictionary<string, string>> ResolveProductNamesAsync(BillingModel bill)
        {
            var productIds = bill.Products.Select(p => p.ProductId).ToList();
            if (productIds.Count == 0)
                return new Dictionary<string, string>();

            var products = await _inventory.GetAllProductByIdsAsync(productIds);
            return products.ToDictionary(p => p.Id, p => p.Name);
        }
    }
}
