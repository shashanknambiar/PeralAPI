using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers.InventoryControllers
{
    [ApiController]
    [Route("api/inventory/products")]
    [Authorize]
    [Tags("Products")]
    public class ProductsController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public ProductsController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        [HttpPost()]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
        {
#warning add a quick search products to avoid creating products with the same name
#warning check for products with same name.
#warning check for valid vendors
            var created = await _inventory.CreateProductAsync(dto);
            var vendors = await _inventory.GetVendorByIdAsync(created.VendorIds);
#warning Add data from products quantity view
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created.ToDto(vendors, 0));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        [ProducesResponseType(typeof(ProductDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductDto?>> UpdateProduct(string id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _inventory.UpdateProductAsync(id, dto);

            if (updated is null)
                return NotFound();
            var vendors = await _inventory.GetVendorByIdAsync(updated.VendorIds);
#warning Add data from products quantity view
            return Ok(updated.ToDto(vendors, 0));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct(string id)
        {
            var deleted = await _inventory.DeleteProductAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpGet("quick-search")]
        [Authorize(Roles = "Inventory Manager, Billing")]

        public async Task<List<ProductSummaryDto>> SearchProductSummary(string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            return (await _inventory.SearchProductAsync(searchString, page, pageSize)).Select(s => s.ToProductSummaryDto()).ToList();
        }

        [HttpGet("search")]
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
        [HttpGet()]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<List<ProductDto>>> GetProduct([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var products = await _inventory.SearchProductAsync("", page, pageSize);
            var vendors = await _inventory.GetVendorByIdAsync(products.SelectMany(p => p.VendorIds).Distinct().ToList());
#warning Add data from products quantity view
            var productsDto = products.Select(p =>
            {
                var resolvedVendors = vendors.Where(v => p.VendorIds.Contains(v.Id)).ToList();
                return p.ToDto(resolvedVendors, 0);
            }).ToList();

            return Ok(productsDto);
        }

        [HttpGet("{id}")]
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

    }
}
