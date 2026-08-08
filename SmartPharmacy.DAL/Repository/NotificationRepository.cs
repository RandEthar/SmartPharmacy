using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
