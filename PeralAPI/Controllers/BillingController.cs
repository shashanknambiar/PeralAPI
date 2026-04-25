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

       
    }
}
