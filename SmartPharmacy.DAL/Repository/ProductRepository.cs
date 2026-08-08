using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
     
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
                     
        }

        public async Task<List<Product>?> DecreaseQuantity(List<OrderItem> orderItems)
        {
            var productIds = orderItems.Select(oi => oi.ProductId).ToList();
            var products = await GetAllAsync(p => productIds.Contains(p.Id),
                new[] { nameof(Product.ProductTranslations) });
            foreach (var item in orderItems)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }
            await UpdateRangAsync(products);
            return products.Where(p => p.StockQuantity<p.MinimumStock).ToList();
        }
          
    }
}
