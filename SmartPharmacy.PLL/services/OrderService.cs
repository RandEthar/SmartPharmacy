using Mapster;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        private static readonly string[] _orderIncludes = new[]
        {
            nameof(Order.OrderItems),
            $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}",
            $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}.{nameof(Product.ProductTranslations)}"
        };

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderResponse>> GetUserOrders(string userId)
        {
            var orders = await _orderRepository.GetAllAsync(
                o => o.UserId == userId,
                _orderIncludes);

            return orders.Adapt<List<OrderResponse>>();
        }

        public async Task<OrderResponse?> GetOrder(string userId, int orderId)
        {
            var order = await _orderRepository.GetOne(
                o => o.UserId == userId && o.Id == orderId,
                _orderIncludes);

            return order?.Adapt<OrderResponse>();
        }

        public async Task<bool> CancelOrder(string userId, int orderId)
        {
            var order = await _orderRepository.GetOne(
                filter: o => o.UserId == userId && o.Id == orderId);

            if (order == null) return false;
            if (order.OrderStatus != OrderStatusEnum.Pending) return false;

            order.OrderStatus = OrderStatusEnum.Cancelled;
            return await _orderRepository.UpdateAsync(order);
        }

        public async Task<List<OrderResponse>> GetAllOrders(OrderStatusEnum status)
        {
            var orders = await _orderRepository.GetAllAsync(
                o => o.OrderStatus == status,
                _orderIncludes);

            return orders.Adapt<List<OrderResponse>>();
        }

        public async Task<OrderResponse?> ChangeOrderState(int orderId, UpdateOrderStatusRequest request)
        {
            var order = await _orderRepository.GetOne(
                o => o.Id == orderId,
                _orderIncludes);

            if (order == null) return null;
            if (order.OrderStatus == OrderStatusEnum.Cancelled || order.OrderStatus == OrderStatusEnum.Delivered)
                return null;
            if (order.OrderStatus == request.OrderStatus) return null;

            order.OrderStatus = request.OrderStatus;
            var updated = await _orderRepository.UpdateAsync(order);

            return updated ? order.Adapt<OrderResponse>() : null;
        }
    }
}
