using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;

namespace SmartPharmacy.PLL.services
{
    public class NotificationService : INotificationService
    {
        private static readonly string[] NotificationIncludes =
        {
            nameof(Notification.NotificationProducts),
            $"{nameof(Notification.NotificationProducts)}.{nameof(NotificationProduct.Product)}",
            $"{nameof(Notification.NotificationProducts)}.{nameof(NotificationProduct.Product)}.{nameof(Product.ProductTranslations)}"
        };

        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            INotificationRepository notificationRepository,
            UserManager<ApplicationUser> userManager)
        {
            _notificationRepository = notificationRepository;
            _userManager = userManager;
        }


        public async Task NotifyPharmacists(NotificationTypeEnum type, List<int> productIds)
        {
            if (productIds is null || !productIds.Any())
                return;

            var pharmacists = await _userManager.GetUsersInRoleAsync(Roles.Pharmacist);
            var message = BuildMessage(type, productIds.Count);

            foreach (var pharmacist in pharmacists)
            {
                // Each pharmacist keeps a single unread notification per type, refreshed in
                // place, so a daily run never piles up duplicates for the same problem.
                var notification = await _notificationRepository.GetOne(
                    n => n.UserId == pharmacist.Id && n.Type == type && !n.IsRead,
                    new[] { nameof(Notification.NotificationProducts) });

                if (notification is null)
                {
                    notification = new Notification
                    {
                        UserId = pharmacist.Id,
                        Type = type,
                        Message = message,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    AttachProducts(notification, productIds);

                    await _notificationRepository.CreateAsync(notification);
                }
                else
                {
                    notification.Message = message;
                    notification.CreatedAt = DateTime.UtcNow;

                    SyncProducts(notification, productIds);

                    await _notificationRepository.UpdateAsync(notification);
                }
            }
        }


        public async Task NotifyUser(string userId, NotificationTypeEnum type, int orderId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            // Unlike the pharmacist alerts, these are never folded into a single unread row:
            // each step an order goes through is a separate event the patient should still see
            // even if they never opened the previous one.
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = BuildUserMessage(type, orderId),
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateAsync(notification);
        }


        public async Task<List<NotificationResponse>> GetUserNotifications(
            string userId, bool unreadOnly = false)
        {
            var notifications = await _notificationRepository.GetAllAsync(
                n => n.UserId == userId && (!unreadOnly || !n.IsRead),
                NotificationIncludes);

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToList()
                .Adapt<List<NotificationResponse>>();
        }


        public async Task<int> GetUnreadCount(string userId)
        {
            return await _notificationRepository
                .GetQueryableAsync(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }


        public async Task<bool> MarkAsRead(string userId, int notificationId)
        {
            // Filtering on UserId as well keeps one user from reading another user's notification.
            var notification = await _notificationRepository.GetOne(
                n => n.Id == notificationId && n.UserId == userId);

            if (notification is null)
                return false;

            if (notification.IsRead)
                return true;

            notification.IsRead = true;

            return await _notificationRepository.UpdateAsync(notification);
        }


        private static void AttachProducts(Notification notification, List<int> productIds)
        {
            foreach (var productId in productIds.Distinct())
            {
                notification.NotificationProducts.Add(new NotificationProduct
                {
                    ProductId = productId
                });
            }
        }


        // The link row is keyed on (NotificationId, ProductId), so clearing the collection and
        // re-adding a product that is still on the list would leave EF tracking two rows with the
        // same key. Only the products that actually came or went are touched.
        private static void SyncProducts(Notification notification, List<int> productIds)
        {
            var wanted = productIds.ToHashSet();

            var removed = notification.NotificationProducts
                .Where(np => !wanted.Contains(np.ProductId))
                .ToList();

            foreach (var link in removed)
                notification.NotificationProducts.Remove(link);

            var alreadyLinked = notification.NotificationProducts
                .Select(np => np.ProductId)
                .ToHashSet();

            foreach (var productId in wanted.Where(id => !alreadyLinked.Contains(id)))
            {
                notification.NotificationProducts.Add(new NotificationProduct
                {
                    ProductId = productId
                });
            }
        }


        private static string BuildMessage(NotificationTypeEnum type, int count) => type switch
        {
            NotificationTypeEnum.LowStock =>
                $"{count} product(s) reached the minimum stock level and need restocking.",

            NotificationTypeEnum.NearExpiry =>
                $"{count} product(s) are approaching their expiry date.",

            NotificationTypeEnum.Expired =>
                $"{count} product(s) have expired and must be removed from the shelves.",

            _ => $"{count} product(s) need your attention."
        };


        private static string BuildUserMessage(NotificationTypeEnum type, int orderId) => type switch
        {
            NotificationTypeEnum.OrderPaid =>
                $"Your order #{orderId} has been paid and is now being prepared.",

            NotificationTypeEnum.OrderShipped =>
                $"Your order #{orderId} is on its way.",

            NotificationTypeEnum.OrderDelivered =>
                $"Your order #{orderId} has been delivered.",

            NotificationTypeEnum.OrderCancelled =>
                $"Your order #{orderId} has been cancelled.",

            NotificationTypeEnum.PrescriptionApproved =>
                $"The prescription for order #{orderId} was approved. You can now complete the payment.",

            NotificationTypeEnum.PrescriptionRejected =>
                $"The prescription for order #{orderId} was rejected, so the order has been cancelled.",

            _ => $"There is an update on your order #{orderId}."
        };
    }
}
