using SmartPharmacy.DAL.DTO.Request;
using System;
using System.Collections.Generic;

namespace SmartPharmacy.DAL.Models
{
    public class Order: AuditableEntity
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public OrderStatusEnum OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public String? StripeSessionId { get; set; }
        public String PhoneNumber { get; set; }
        public String City { get; set; }
        public String Street { get; set; }

        public String UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
