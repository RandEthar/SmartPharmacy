using Microsoft.EntityFrameworkCore;

namespace SmartPharmacy.DAL.Models
{
    [PrimaryKey(nameof(UserId), nameof(ProductId))]
    public class CartItem
    {
        public String UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
    }
}
