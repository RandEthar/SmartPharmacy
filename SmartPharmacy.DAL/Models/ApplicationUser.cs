using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Models
{
    public class ApplicationUser:IdentityUser
    {
        public String FullName { get; set; }
        public String? City { get; set; }
        public String? Street { get; set; }
        //عشان نقارن الكود يلي بعتناه بالكود يلي كتبه اليوزر منخزنه بجدول اليوزر
        public String? CodeResetPassword { get; set; }
        //عشان نحدد وقت انتهاء صلاحية الكود يلي بعتناه لليوزر
        public DateTime? CodeResetPasswordExpiration { get; set; }
        public String? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
