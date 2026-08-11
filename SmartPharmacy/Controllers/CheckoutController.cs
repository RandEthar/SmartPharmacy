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

        // Anonymous because Stripe redirects the browser here without the bearer token.
        // Safe now only because ConfirmPayment verifies the payment against Stripe itself.
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

        // Stripe calls this server-to-server, so the order is still confirmed even if the
        // customer closes the tab before the success redirect fires. Authenticated by the
        // Stripe-Signature header rather than by a JWT.
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            var requestBody = await reader.ReadToEndAsync();

            var response = await _checkoutService.HandleStripeWebhook(
                requestBody,
                Request.Headers["Stripe-Signature"]);

            if (!response.Success)
            {
                return BadRequest(response.ErrorMessage);
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            return Ok(new { message = "Payment was cancelled." });
        }
    }
}
