using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Repository
{
    public class CartRepository : GenericRepository<CartItem>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
