using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public InventoryController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        #region Vendors

        [HttpPost("vendors")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
        {
            var created = await _inventory.CreateVendorAsync(dto);
            return CreatedAtAction(nameof(CreateVendor), new { id = created.Id }, created);
        }

        [HttpGet("vendors")]
        public async Task<IActionResult> GetAllVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vendors = await _inventory.GetAllVendorsAsync(page, pageSize);
            return Ok(vendors.Select(v => v.ToDto()));
        }

        [HttpGet("vendors/search")]
        public async Task<IActionResult> SearchVendors([FromQuery] string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var results = await _inventory.SearchVendorsAsync(searchString, page, pageSize);
            return Ok(results.Select(v => v.ToDto()));
        }
        [HttpGet("vendors/{id}")]
        public async Task<IActionResult> GetVendorById(string id)
        {
            var vendor = await _inventory.GetVendorByIdAsync(id);

            if (vendor is null)
                return NotFound();

            return Ok(vendor.ToDto());
        }
        [HttpPut("vendors/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> UpdateVendor(string id, [FromBody] UpdateVendorDto dto)
        {
            var updated = await _inventory.UpdateVendorAsync(id, dto);

            if (updated is null)
                return NotFound();
#warning fetch credit here.
            return Ok(updated.ToDto());
        }

        [HttpDelete("vendors/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> DeleteVendor(string id)
        {
            var deleted = await _inventory.DeleteVendorAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { message = "Vendor deleted." });
        }

        #endregion



        #region Products

        [HttpPost("products")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
#warning add a quick search products to avoid creating products with the same name
#warning check for products with same name.
#warning check for valid vendors
            var created = await _inventory.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
        }

        [HttpPut("products/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _inventory.UpdateProductAsync(id, dto);

            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("products/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var deleted = await _inventory.DeleteProductAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { message = "Product deleted." });
        }

        [HttpGet("products/quick-search")]
        [Authorize(Roles = "Inventory Manager, Billing")]

        public async Task<List<ProductSummaryDto>> SearchProductSummary(string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return (await _inventory.SearchProductAsync(searchString, page, pageSize)).Select(s => s.ToProductSummaryDto()).ToList();
        }

        [HttpGet("products/search")]
        [Authorize(Roles = "Inventory Manager, Billing")]

        public async Task<List<ProductDto>> SearchProductsAsync(string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var products = await _inventory.SearchProductAsync(searchString, page, pageSize);
            var vendors = await _inventory.GetVendorByIdAsync(products.SelectMany(p => p.VendorIds).Distinct().ToList());
#warning Add data from products quantity view
            var productsDto = products.Select(p =>
            {
                var resolvedVendors = vendors.Where(v => p.VendorIds.Contains(v.Id)).ToList();
                return p.ToDto(resolvedVendors, 0);
            }).ToList();

            return productsDto;

        }
        [HttpGet("products")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<List<ProductDto>> GetProduct([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var products = await _inventory.SearchProductAsync("", page, pageSize);
            var vendors = await _inventory.GetVendorByIdAsync(products.SelectMany(p => p.VendorIds).Distinct().ToList());
#warning Add data from products quantity view
            var productsDto = products.Select(p =>
            {
                var resolvedVendors = vendors.Where(v => p.VendorIds.Contains(v.Id)).ToList();
                return p.ToDto(resolvedVendors, 0);
            }).ToList();

            return productsDto;
        }

        [HttpGet("products/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _inventory.GetProductByIdAsync(id);
            if (product == null)
                return NotFound("Product not found.");
            var vendors = await _inventory.GetVendorByIdAsync(product.VendorIds);
#warning Add data from products quantity view
            return Ok(product.ToDto(vendors, 0));
        }

        #endregion


        #region Orders

        [HttpPost("orders")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> CreateInventoryOrder([FromBody] CreateInventoryOrderDto dto)
        {
            {
                var result = await _inventory.CreateInventoryOrderAsync(dto);
                return Ok(result);
            }
        #endregion

        }

        [HttpPut("orders/change-status/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> UpdateInventoryOrderStatus(string id, [FromBody] ChangeInventoryOrderStatusDto dto)
        {
            var result = await _inventory.ChangeInventoryOrderStatus(dto);
            if (result == null)
                return NotFound("Order not found.");
            return Ok(result);
        }

        [HttpPut("orders/{id}")]
        [Authorize(Roles ="Inventory Manager")]
        public async Task<IActionResult> UpdateInventoryOrder(string id, [FromBody] UpdateInventoryOrderDto dto)
        {
            if(id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");
            var result = await _inventory.UpdateInventoryOrderAsync(dto);
            if (result == null)
                return NotFound("Order not found.");
            return Ok(result);
        }

        [HttpGet("orders/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> GetInventoryOrderById(string id)
        {
            var result = await _inventory.GetInventoryOrderByIdAsync(id);
            if (result == null)
                return NotFound("Order not found.");
            return Ok(result);
        }

        [HttpGet("orders/search")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> SearchInventoryOrders([FromQuery] string query = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _inventory.SearchInventoryOrdersAsync(query, page, pageSize);
            return Ok(result);
        }

        [HttpGet("orders")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> GetInventoryOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _inventory.SearchInventoryOrdersAsync("", page, pageSize);
            return Ok(result);
        }
    }
}
