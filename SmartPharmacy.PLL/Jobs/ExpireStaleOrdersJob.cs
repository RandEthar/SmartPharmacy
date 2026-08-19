using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;
using SmartPharmacy.PLL.services;

namespace SmartPharmacy.PLL.Jobs
{
    /// <summary>
    /// Stock is reserved the moment an order is created, so an order nobody ever pays for would
    /// keep medicine off the shelf indefinitely. This releases those reservations.
    /// </summary>
    public class ExpireStaleOrdersJob : IExpireStaleOrdersJob
    {
        private const int DefaultUnpaidExpiryHours = 24;
        private const int DefaultPrescriptionExpiryHours = 72;

        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpireStaleOrdersJob> _logger;
        private readonly int _unpaidExpiryHours;
        private readonly int _prescriptionExpiryHours;

        public ExpireStaleOrdersJob(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<ExpireStaleOrdersJob> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;

            //"Orders": {
            //    "UnpaidExpiryHours": 24,
            //    "PrescriptionExpiryHours": 72
            //     },

    
            _unpaidExpiryHours =
                int.TryParse(configuration["Orders:UnpaidExpiryHours"], out var unpaid)
                    ? unpaid
                    : DefaultUnpaidExpiryHours;

         
            _prescriptionExpiryHours =
                int.TryParse(configuration["Orders:PrescriptionExpiryHours"], out var prescription)
                    ? prescription
                    : DefaultPrescriptionExpiryHours;
        }

        public async Task Run()
        {
            var now = DateTime.UtcNow;
         
            await ExpireOrders(OrderStatusEnum.Pending, now.AddHours(-_unpaidExpiryHours));
            await ExpireOrders(OrderStatusEnum.AwaitingPrescription, now.AddHours(-_prescriptionExpiryHours));
        }

        private async Task ExpireOrders(OrderStatusEnum status, DateTime cutoff)
        {
            var staleOrders = await _orderRepository.GetAllAsync(
                o => o.OrderStatus == status && o.OrderDate < cutoff,
                new[] { nameof(Order.OrderItems) });

            foreach (var order in staleOrders)
            {
               
                try
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    order.OrderStatus = OrderStatusEnum.Cancelled;
                    await _orderRepository.UpdateAsync(order);

                 
                    await _productRepository.RestoreStock(order.OrderItems);

                    await transaction.CommitAsync();

                    // The patient did not ask for this, so of all the cancellation paths this is
                    // the one they most need to be told about.
                    await _notificationService.NotifyUser(
                        order.UserId, NotificationTypeEnum.OrderCancelled, order.Id);

                    _logger.LogInformation(
                        "Cancelled stale order {OrderId} ({Status}) and released its reserved stock.",
                        order.Id, status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not expire stale order {OrderId}.", order.Id);
                }
            }
        }
    }
}
