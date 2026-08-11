using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;

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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExpireStaleOrdersJob> _logger;
        private readonly int _unpaidExpiryHours;
        private readonly int _prescriptionExpiryHours;

        public ExpireStaleOrdersJob(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<ExpireStaleOrdersJob> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;

            _unpaidExpiryHours =
                int.TryParse(configuration["Orders:UnpaidExpiryHours"], out var unpaid)
                    ? unpaid
                    : DefaultUnpaidExpiryHours;

            // Longer, because this one is waiting on a pharmacist rather than on the customer.
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
                // One transaction per order: a single failure must not undo the releases that
                // already succeeded, nor stop the remaining orders from being processed.
                try
                {
                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    order.OrderStatus = OrderStatusEnum.Cancelled;
                    await _orderRepository.UpdateAsync(order);

                    // After the save, so the tracked order graph cannot write a stale quantity
                    // back over the restored stock.
                    await _productRepository.RestoreStock(order.OrderItems);

                    await transaction.CommitAsync();

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
