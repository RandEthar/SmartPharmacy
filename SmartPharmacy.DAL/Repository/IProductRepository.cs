using SmartPharmacy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        Task<List<Product>?> DecreaseQuantity(List<OrderItem> orderItems);
    }
}
