using Mapster;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Extentions;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;

        private static readonly string[] _orderIncludes = new[]
        {
            nameof(Order.OrderItems),
            $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}",
            $"{nameof(Order.OrderItems)}.{nameof(OrderItem.Product)}.{nameof(Product.ProductTranslations)}"
        };

        // Stock is taken when the order is created and only released when it is delivered,
        // so every other state still holds it and must give it back if the order is cancelled.
        private static bool HoldsStock(OrderStatusEnum status) =>
            status != OrderStatusEnum.Cancelled && status != OrderStatusEnum.Delivered;

        // Only the transitions the patient actually cares about. Paid is deliberately absent:
        // the checkout flow already raises it the moment the payment clears.
        private static NotificationTypeEnum? MapStatusToNotification(OrderStatusEnum status) => status switch
        {
            OrderStatusEnum.Shipped => NotificationTypeEnum.OrderShipped,
            OrderStatusEnum.Delivered => NotificationTypeEnum.OrderDelivered,
            OrderStatusEnum.Cancelled => NotificationTypeEnum.OrderCancelled,
            _ => null
        };

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
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
                filter: o => o.UserId == userId && o.Id == orderId,
                inclead: new[] { nameof(Order.OrderItems) });

            if (order == null) return false;

            // AwaitingPrescription is cancellable too - it is still unpaid, and leaving it
            // uncancellable would keep its stock reserved forever if the patient walks away.
            if (order.OrderStatus != OrderStatusEnum.Pending &&
                order.OrderStatus != OrderStatusEnum.AwaitingPrescription)
                return false;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            order.OrderStatus = OrderStatusEnum.Cancelled;
            var updated = await _orderRepository.UpdateAsync(order);

            // Deliberately after the save: UpdateAsync writes back every tracked entity in the
            // order graph, which would overwrite a stock change made before it with a stale value.
            await _productRepository.RestoreStock(order.OrderItems);

            await transaction.CommitAsync();
            return updated;
        }

        public async Task<PagenationResponse<OrderResponse>> GetAllOrders(
            OrderStatusEnum status, PagenationRequest request)
        {
            var query = _orderRepository
                .GetQueryableAsync(o => o.OrderStatus == status, _orderIncludes)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.Id);

            var orders = await query.ApplyPagenation(request.Page, request.Limit);

            return new PagenationResponse<OrderResponse>
            {
                Data = orders.Data.Adapt<List<OrderResponse>>(),
                TotalCount = orders.TotalCount,
                Page = orders.Page,
                Limit = orders.Limit
            };
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

            // Cancelling from the back office used to leave the goods permanently subtracted,
            // so the recorded stock drifted below what was actually on the shelf.
            var releasesStock = request.OrderStatus == OrderStatusEnum.Cancelled
                                && HoldsStock(order.OrderStatus);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            order.OrderStatus = request.OrderStatus;
            var updated = await _orderRepository.UpdateAsync(order);

            // Must run after the save. This query includes OrderItems.Product, so those products
            // are tracked with their pre-restore quantity; saving the graph afterwards would
            // write that stale number straight back over the restored stock.
            if (releasesStock)
            {
                await _productRepository.RestoreStock(order.OrderItems);
            }

            await transaction.CommitAsync();

            // After the commit: telling the patient is a side effect, and a failure here must
            // not roll back a status change the pharmacy already made.
            if (updated)
            {
                var patientNotification = MapStatusToNotification(request.OrderStatus);
                if (patientNotification.HasValue)
                {
                    await _notificationService.NotifyUser(
                        order.UserId, patientNotification.Value, order.Id);
                }
            }

            return updated ? order.Adapt<OrderResponse>() : null;
        }
    }
}
