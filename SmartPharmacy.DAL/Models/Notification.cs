using System;
using System.Collections.Generic;

namespace SmartPharmacy.DAL.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public String Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public NotificationTypeEnum Type { get; set; }

        public String UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<NotificationProduct> NotificationProducts { get; set; } = new List<NotificationProduct>();
    }
}
