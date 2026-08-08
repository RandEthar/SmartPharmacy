using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class OrderResponse
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentMethod { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public List<OrderItemResponse> Items { get; set; }
        public decimal Total => Items?.Sum(i => i.Subtotal) ?? 0;
    }
}
