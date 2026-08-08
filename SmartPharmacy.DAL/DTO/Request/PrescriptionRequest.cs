using Microsoft.AspNetCore.Http;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class PrescriptionRequest
    {
        public int OrderId { get; set; }
        public IFormFile Image { get; set; }
    }
}
