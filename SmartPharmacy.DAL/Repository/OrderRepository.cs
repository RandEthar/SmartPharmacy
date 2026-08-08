using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Repository
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
