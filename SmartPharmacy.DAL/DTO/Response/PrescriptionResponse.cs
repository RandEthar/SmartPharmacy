using System;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class PrescriptionResponse
    {
   
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int OrderId { get; set; }
    }
}
