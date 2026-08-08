using System;

namespace SmartPharmacy.DAL.Models
{
    public class Prescription: AuditableEntity
    {
        public int Id { get; set; }
        public String ImageUrl { get; set; }
        public PrescriptionStatusEnum Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
