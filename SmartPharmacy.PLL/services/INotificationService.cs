using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.PLL.services
{
    public interface INotificationService
    {
        Task NotifyPharmacists(NotificationTypeEnum type, List<int> productIds);
        Task<List<NotificationResponse>> GetUserNotifications(string userId, bool unreadOnly = false);
        Task<int> GetUnreadCount(string userId);
        Task<bool> MarkAsRead(string userId, int notificationId);
    }
}
