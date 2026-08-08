using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.PLL.services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;

        public CheckoutController(ICheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
        {
            var response = await _checkoutService.Checkout(UserId, request);
            if (!response.Success)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response);
        }

        [HttpPost("{orderId}/pay")]
        public async Task<IActionResult> PayOrder(int orderId)
        {
            var response = await _checkoutService.PayOrder(UserId, orderId);
            if (!response.Success)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("success")]
        public async Task<IActionResult> Success(string sessionId)
        {
            var response = await _checkoutService.ConfirmPayment(sessionId);
            if (!response.Success)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok(new { message = "Payment was cancelled." });
        }
    }
}
