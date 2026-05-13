using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
            var quantity = await _inventory.GetProductStockByIdsAsync(new List<string> { created.Id });
            quantity.TryGetValue(created.Id, out var stock);
            return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created.ToDto(vendors, stock));
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
            var quantity = await _inventory.GetProductStockByIdsAsync(new List<string> { updated.Id });
            quantity.TryGetValue(updated.Id, out var stock);
            return Ok(updated.ToDto(vendors, stock));
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
        public async Task<ActionResult<List<ProductDto>>> SearchProductsAsync(string searchString = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var products = await _inventory.SearchProductAsync(searchString, page, pageSize);
            var productIds = products.Select(p => p.Id).ToList();
            var vendorIds = products.SelectMany(s => s.VendorIds).ToList();
            var vendorsTask = _inventory.GetVendorByIdAsync(vendorIds);
            var quantityTask = _inventory.GetProductStockByIdsAsync(productIds);
            var placedOrdersTask = _inventory.GetPlacedOrderIdsByProductIdsAsync(productIds);
            await Task.WhenAll(vendorsTask, quantityTask, placedOrdersTask);
            var vendors = vendorsTask.Result;
            var quantity = quantityTask.Result;
            var placedOrders = placedOrdersTask.Result;
            var productsDto = products.Select(p =>
            {
                var resolvedVendors = vendors.Where(v => p.VendorIds.Contains(v.Id)).ToList();
                quantity.TryGetValue(p.Id, out var stock);
                placedOrders.TryGetValue(p.Id, out var placedOrderId);
                return p.ToDto(resolvedVendors, stock, placedOrderId);
            }).ToList();

            return Ok(productsDto);
        }

        [HttpGet()]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<List<ProductDto>>> GetProduct([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var products = await _inventory.SearchProductAsync("", page, pageSize);
            var productIds = products.Select(p => p.Id).ToList();
            var vendorIds = products.SelectMany(s => s.VendorIds).ToList();
            var vendorsTask = _inventory.GetVendorByIdAsync(vendorIds);
            var quantityTask = _inventory.GetProductStockByIdsAsync(productIds);
            var placedOrdersTask = _inventory.GetPlacedOrderIdsByProductIdsAsync(productIds);
            await Task.WhenAll(vendorsTask, quantityTask, placedOrdersTask);
            var vendors = vendorsTask.Result;
            var quantity = quantityTask.Result;
            var placedOrders = placedOrdersTask.Result;
            var productsDto = products.Select(p =>
            {
                var resolvedVendors = vendors.Where(v => p.VendorIds.Contains(v.Id)).ToList();
                quantity.TryGetValue(p.Id, out var stock);
                placedOrders.TryGetValue(p.Id, out var placedOrderId);
                return p.ToDto(resolvedVendors, stock, placedOrderId);
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
            var vendorsTask = _inventory.GetVendorByIdAsync(product.VendorIds);
            var quantityTask = _inventory.GetProductStockByIdsAsync(new List<string> { product.Id });
            var placedOrdersTask = _inventory.GetPlacedOrderIdsByProductIdsAsync(new List<string> { product.Id });
            await Task.WhenAll(vendorsTask, quantityTask, placedOrdersTask);
            quantityTask.Result.TryGetValue(product.Id, out var stock);
            placedOrdersTask.Result.TryGetValue(product.Id, out var placedOrderId);
            return Ok(product.ToDto(vendorsTask.Result, stock, placedOrderId));
        }

    }
}
