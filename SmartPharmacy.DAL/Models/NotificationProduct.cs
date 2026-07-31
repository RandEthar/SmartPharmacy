using Microsoft.EntityFrameworkCore;

namespace SmartPharmacy.DAL.Models
{
    [PrimaryKey(nameof(NotificationId), nameof(ProductId))]
    public class NotificationProduct
    {
        public int NotificationId { get; set; }
        public Notification Notification { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
