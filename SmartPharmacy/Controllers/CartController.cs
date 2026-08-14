using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.PLL.services;
using System.Security.Claims;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var cartResponse = await _cartService.GetCart(UserId);
            return Ok(cartResponse);
        }

        // No try/catch here any more: GlobalExceptionHandler turns the InvalidOperationException
        // the cart raises for an expired or out-of-stock item into a 400 ProblemDetails.
        [HttpPost]
        public async Task<IActionResult> AddToCart(CartItemRequest request)
        {
            var cartItemResponse = await _cartService.AddToCart(UserId, request);
            if (cartItemResponse == null)
            {
                return Problem(detail: $"Product with id {request.ProductId} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(cartItemResponse);
        }

        [HttpPatch("{productId}")]
        public async Task<IActionResult> UpdateCartItem(int productId, UpdateCartItemRequest request)
        {
            var cartItemResponse = await _cartService.UpdateQuantity(UserId, productId, request);
            if (cartItemResponse == null)
            {
                return Problem(detail: $"Cart item with product id {productId} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(cartItemResponse);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var removed = await _cartService.RemoveFromCart(UserId, productId);
            if (!removed)
            {
                return Problem(detail: $"Cart item with product id {productId} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            await _cartService.ClearCart(UserId);
            return Ok();
        }
    }
}
