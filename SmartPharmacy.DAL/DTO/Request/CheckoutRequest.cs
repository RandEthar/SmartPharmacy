using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SmartPharmacy.DAL.DTO.Request
{

    public enum PaymentMethod
    {
        Cash = 1,
        Visa = 2,
    }
    public class CheckoutRequest
    {
        //لما يجي الـ JSON أو لما نرجع Response، استخدم أسماء الـ Enum ("Cash" و "Visa") بدل القيم الرقمية (1 و 2)
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; } 
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
       
    }
}
