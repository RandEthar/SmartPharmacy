using Microsoft.EntityFrameworkCore;
using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {

        public ProductRepository(ApplicationDbContext context) : base(context)
        {

        }

        public async Task<bool> TryReserveStock(IEnumerable<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                var productId = item.ProductId;
                var quantity = item.Quantity;

                // Read-then-subtract in C# is what allowed two simultaneous checkouts to both
                // pass the stock check and drive the quantity negative. A single conditional
                // UPDATE makes the database do the check and the subtraction under one row lock:
                // if another order got there first, zero rows match and this returns false.
                var affected = await _context.Products
                    .Where(p => p.Id == productId && p.StockQuantity >= quantity)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(p => p.StockQuantity, p => p.StockQuantity - quantity));

                if (affected == 0)
                    return false;
            }

            return true;
        }

        public async Task RestoreStock(IEnumerable<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                var productId = item.ProductId;
                var quantity = item.Quantity;

                await _context.Products
                    .Where(p => p.Id == productId)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(p => p.StockQuantity, p => p.StockQuantity + quantity));
            }
        }

        public async Task<List<Product>> GetProductsBelowMinimumStock(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();

            return await _context.Products
                .Include(p => p.ProductTranslations)
                .Where(p => ids.Contains(p.Id) && p.StockQuantity <= p.MinimumStock)
                .ToListAsync();
        }
    }
}
