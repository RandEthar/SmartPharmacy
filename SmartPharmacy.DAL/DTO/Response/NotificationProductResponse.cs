using System;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class NotificationProductResponse
    {
        public int ProductId { get; set; }
        public String Name { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public DateOnly ExpiryDate { get; set; }
    }
}
