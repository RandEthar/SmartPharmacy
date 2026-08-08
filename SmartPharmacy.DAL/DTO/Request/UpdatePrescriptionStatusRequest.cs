using SmartPharmacy.DAL.Models;
using System.Text.Json.Serialization;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class UpdatePrescriptionStatusRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PrescriptionStatusEnum Status { get; set; }
    }
}
