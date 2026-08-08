using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.PLL.services;
using System.Security.Claims;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            var orders = await _orderService.GetUserOrders(UserId);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _orderService.GetOrder(UserId, id);
            if (order == null)
            {
                return Problem(detail: $"Order with id {id} was not found.", statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(order);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var cancelled = await _orderService.CancelOrder(UserId, id);
            if (!cancelled)
            {
                return BadRequest("Order cannot be cancelled.");
            }
            return Ok();
        }

        [HttpGet("admin")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] OrderStatusEnum status)
        {
            var orders = await _orderService.GetAllOrders(status);
            return Ok(orders);
        }

        [HttpPatch("{id}/status")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeOrderState(int id, UpdateOrderStatusRequest request)
        {
            var order = await _orderService.ChangeOrderState(id, request);
            if (order == null)
            {
                return BadRequest("Order status cannot be changed.");
            }
            return Ok(order);
        }
    }
}
