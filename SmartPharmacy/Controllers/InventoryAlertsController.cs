using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.PLL.services;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Pharmacist,Admin")]
    public class InventoryAlertsController : ControllerBase
    {
        private readonly IInventoryAlertService _inventoryAlertService;

        public InventoryAlertsController(IInventoryAlertService inventoryAlertService)
        {
            _inventoryAlertService = inventoryAlertService;
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var alerts = await _inventoryAlertService.GetLowStockProducts();
            return Ok(alerts);
        }

        [HttpGet("near-expiry")]
        public async Task<IActionResult> GetNearExpiry()
        {
            var alerts = await _inventoryAlertService.GetNearExpiryProducts();
            return Ok(alerts);
        }

        [HttpGet("expired")]
        public async Task<IActionResult> GetExpired()
        {
            var alerts = await _inventoryAlertService.GetExpiredProducts();
            return Ok(alerts);
        }
    }
}
