using System.Collections.Generic;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class CartResponse
    {
        public List<CartItemResponse> Items { get; set; }
        public decimal Total { get; set; }
    }
}
