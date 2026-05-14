using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PeralAPI.Models.DTOs;
using PeralAPI.Models.Inventory;
using PeralAPI.Services.Inventory;

namespace PeralAPI.Controllers.InventoryControllers
{
    [ApiController]
    [Route("api/inventory/orders")]
    [Tags("Orders")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IInventoryService _inventory;

        public OrdersController(IInventoryService inventory)
        {
            _inventory = inventory;
        }

        [HttpPost]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<InventoryOrderDto>> CreateInventoryOrder([FromBody] CreateInventoryOrderDto dto)
        {
            {
                var order = await _inventory.CreateInventoryOrderAsync(dto);
                var getVendorsTask = _inventory.GetAllVendorByIdAsync(order.VendorId);
                var getProductsTask = _inventory.GetAllProductByIdsAsync(order.Products.Select(p => p.ProductId).ToList());
                Task.WaitAll(getVendorsTask, getProductsTask);
                var vendor = getVendorsTask.Result;
                var productDic = getProductsTask.Result.ToDictionary(k => k.Id, i => i.Name);
                var products = order.Products.Select(s => new PurchaseItemDto(s.ProductId, productDic[s.ProductId], s.Quantity, s.PricePerItem)).ToList();
                return Ok(order.ToDto(getVendorsTask.Result!.ToDto(), products));
            }
        }

        [HttpPut("change-status/{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<InventoryOrderDto>> UpdateInventoryOrderStatus(string id, [FromBody] ChangeInventoryOrderStatusDto dto)
        {
            var order = await _inventory.ChangeInventoryOrderStatus(dto);
            if (order == null)
                return NotFound("Order not found.");

            var getVendorsTask = _inventory.GetAllVendorByIdAsync(order.VendorId);
            var getProductsTask = _inventory.GetAllProductByIdsAsync(order.Products.Select(p => p.ProductId).ToList());
            Task.WaitAll(getVendorsTask, getProductsTask);
            var vendor = getVendorsTask.Result;
            var productDic = getProductsTask.Result.ToDictionary(k => k.Id, i => i.Name);
            var products = order.Products.Select(s => new PurchaseItemDto(s.ProductId, productDic[s.ProductId], s.Quantity, s.PricePerItem)).ToList();
            return Ok(order.ToDto(getVendorsTask.Result!.ToDto(), products));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<InventoryOrderDto>> UpdateInventoryOrder(string id, [FromBody] UpdateInventoryOrderDto dto)
        {
            if (id != dto.Id)
                return BadRequest("ID in URL does not match ID in body.");
            var order = await _inventory.UpdateInventoryOrderAsync(dto);
            if (order == null)
                return NotFound("Order not found.");
            var getVendorsTask = _inventory.GetAllVendorByIdAsync(order.VendorId);
            var getProductsTask = _inventory.GetAllProductByIdsAsync(order.Products.Select(p => p.ProductId).ToList());
            Task.WaitAll(getVendorsTask, getProductsTask);
            var vendor = getVendorsTask.Result;
            var productDic = getProductsTask.Result.ToDictionary(k => k.Id, i => i.Name);
            var products = order.Products.Select(s => new PurchaseItemDto(s.ProductId, productDic[s.ProductId], s.Quantity, s.PricePerItem)).ToList();
            return Ok(order.ToDto(getVendorsTask.Result!.ToDto(), products));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult> DeleteInventoryOrder(string id)
        {
            try
            {
                var deleted = await _inventory.DeleteInventoryOrderAsync(id);
                if (!deleted)
                    return NotFound("Order not found.");
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<InventoryOrderDto>> GetInventoryOrderById(string id)
        {
            var order = await _inventory.GetInventoryOrderByIdAsync(id);
            if (order == null)
                return NotFound("Order not found.");
            var getVendorsTask = _inventory.GetAllVendorByIdAsync(order.VendorId);
            var getProductsTask = _inventory.GetAllProductByIdsAsync(order.Products.Select(p => p.ProductId).ToList());
            Task.WaitAll(getVendorsTask, getProductsTask);
            var vendor = getVendorsTask.Result;
            var productDic = getProductsTask.Result.ToDictionary(k => k.Id, i => i.Name);
            var products = order.Products.Select(s => new PurchaseItemDto(s.ProductId, productDic[s.ProductId], s.Quantity, s.PricePerItem)).ToList();
            return Ok(order.ToDto(getVendorsTask.Result!.ToDto(), products));
        }

        [HttpGet("search")]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<OrdersSearchResultDto>> SearchInventoryOrders(
            [FromQuery] string? orderId = null,
            [FromQuery] string? vendorName = null,
            [FromQuery] InventoryOrderStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var searchParams = new OrderSearchParamsDto(orderId, vendorName, status, fromDate, toDate);
            var (orders, totalCount) = await _inventory.SearchInventoryOrdersAsync(searchParams, page, pageSize);
            var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;

            List<InventoryOrderDto> orderDtos = new List<InventoryOrderDto>();
            foreach (var order in orders)
            {
                var getVendorsTask = _inventory.GetAllVendorByIdAsync(order.VendorId);
                var getProductsTask = _inventory.GetAllProductByIdsAsync(order.Products.Select(p => p.ProductId).ToList());
                Task.WaitAll(getVendorsTask, getProductsTask);
                var productDic = getProductsTask.Result.ToDictionary(k => k.Id, i => i.Name);
                var products = order.Products.Select(s => new PurchaseItemDto(s.ProductId, productDic[s.ProductId], s.Quantity, s.PricePerItem)).ToList();
                orderDtos.Add(order.ToDto(getVendorsTask.Result!.ToDto(), products));
            }

            return Ok(new OrdersSearchResultDto(page, totalPages, orderDtos));
        }

        [HttpGet]
        [Authorize(Roles = "Inventory Manager")]
        public async Task<ActionResult<List<InventoryOrderDto>>> GetInventoryOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (orders, _) = await _inventory.SearchInventoryOrdersAsync(new OrderSearchParamsDto(), page, pageSize);
            List<InventoryOrderDto> orderDtos = new List<InventoryOrderDto>();
            foreach (var order in orders)
            {
                var getVendorsTask = _inventory.GetAllVendorByIdAsync(order.VendorId);
                var getProductsTask = _inventory.GetAllProductByIdsAsync(order.Products.Select(p => p.ProductId).ToList());
                Task.WaitAll(getVendorsTask, getProductsTask);
                var productDic = getProductsTask.Result.ToDictionary(k => k.Id, i => i.Name);
                var products = order.Products.Select(s => new PurchaseItemDto(s.ProductId, productDic[s.ProductId], s.Quantity, s.PricePerItem)).ToList();
                orderDtos.Add(order.ToDto(getVendorsTask.Result!.ToDto(), products));
            }
            return Ok(orderDtos);
        }
    }
}
