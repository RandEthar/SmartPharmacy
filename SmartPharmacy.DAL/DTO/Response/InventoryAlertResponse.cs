using System;

namespace SmartPharmacy.DAL.DTO.Response
{
    public class InventoryAlertResponse
    {
        public int ProductId { get; set; }
        public String Name { get; set; }
        public String Image { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public DateOnly ExpiryDate { get; set; }

        // Negative once the product is already expired.
        public int DaysUntilExpiry { get; set; }
    }
}
