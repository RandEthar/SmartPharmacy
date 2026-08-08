using SmartPharmacy.DAL.Models;
using System.Text.Json.Serialization;

namespace SmartPharmacy.DAL.DTO.Request
{
    public class UpdateOrderStatusRequest
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatusEnum OrderStatus { get; set; }
    }
}
