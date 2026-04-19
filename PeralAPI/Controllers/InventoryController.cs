using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
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

        /// <summary>Creates a new vendor. Vendor ID and contact IDs are generated server-side. Requires the "Inventory Manager" role.</summary>
        [HttpPost("vendors")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> CreateVendor([FromBody] CreateVendorDto dto)
        {
            var created = await _inventory.CreateVendorAsync(dto);
            return CreatedAtAction(nameof(CreateVendor), new { id = created.Id }, created);
        }

        /// <summary>Returns all vendors, paginated. Requires authentication.</summary>
        [HttpGet("vendors")]
        public async Task<IActionResult> GetAllVendors([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var vendors = await _inventory.GetAllVendorsAsync(page, pageSize);
            return Ok(vendors);
        }

        /// <summary>Searches vendors by name (case-insensitive, partial match), paginated. Requires authentication.</summary>
        [HttpGet("vendors/search")]
        public async Task<IActionResult> SearchVendors(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Query parameter 'q' is required." });

            var results = await _inventory.SearchVendorsAsync(q, page, pageSize);
            return Ok(results);
        }

        /// <summary>Returns a single vendor by ID. Requires authentication.</summary>
        [HttpGet("vendors/{id}")]
        public async Task<IActionResult> GetVendorById(string id)
        {
            var vendor = await _inventory.GetVendorByIdAsync(id);

            if (vendor is null)
                return NotFound();

            return Ok(vendor);
        }

        /// <summary>Updates a vendor by ID. Contact IDs are regenerated server-side. Requires the "Inventory Manager" role.</summary>
        [HttpPut("vendors/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> UpdateVendor(string id, [FromBody] CreateVendorDto dto)
        {
            var updated = await _inventory.UpdateVendorAsync(id, dto);

            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        /// <summary>Deletes a vendor by ID. Requires the "Inventory Manager" role.</summary>
        [HttpDelete("vendors/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> DeleteVendor(string id)
        {
            var deleted = await _inventory.DeleteVendorAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { message = "Vendor deleted." });
        }

        /// <summary>Creates a new product. Product ID and vendor IDs are generated server-side. Requires the "Inventory Manager" role.</summary>
        [HttpPost("products")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            var created = await _inventory.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates a product by ID. Vendor IDs are regenerated server-side. Requires the "Inventory Manager" role.</summary>
        [HttpPut("products/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto dto)
        {
            var updated = await _inventory.UpdateProductAsync(id, dto);

            if (updated is null)
                return NotFound();

            return Ok(updated);
        }

        /// <summary>Deletes a product by ID. Requires the "Inventory Manager" role.</summary>
        [HttpDelete("products/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var deleted = await _inventory.DeleteProductAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { message = "Product deleted." });
        }

        /// <summary>Returns all products, paginated. Requires authentication.</summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var products = await _inventory.GetAllProductsAsync(page, pageSize);
            return Ok(products);
        }

        /// <summary>Returns a single product by ID. Requires authentication.</summary>
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var product = await _inventory.GetProductByIdAsync(id);

            if (product is null)
                return NotFound();

            return Ok(product);
        }

        /// <summary>
        /// Searches products by name (case-insensitive, partial match), paginated.
        /// Requires authentication.
        /// </summary>
        [HttpGet("products/search")]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(new { error = "Query parameter 'q' is required." });

            var results = await _inventory.SearchProductsAsync(q, page, pageSize);
            return Ok(results);
        }

        /// <summary>Creates a new purchase order. Order ID, total quantity, and total purchase value are computed server-side. Requires the "Inventory Manager" role.</summary>
        [HttpPost("purchase-order")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderDto dto)
        {
            var created = await _inventory.CreatePurchaseOrderAsync(dto);
            return StatusCode(201, created);
        }
    }
}
