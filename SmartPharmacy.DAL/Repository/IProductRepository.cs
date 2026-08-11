using SmartPharmacy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        /// <summary>
        /// Takes the ordered quantities out of stock. Returns false - without changing anything
        /// further - as soon as one product does not have enough left, so the caller can roll back.
        /// Must be called inside a transaction.
        /// </summary>
        Task<bool> TryReserveStock(IEnumerable<OrderItem> orderItems);

        /// <summary>
        /// Puts the ordered quantities back, used when an order that held stock is cancelled.
        /// </summary>
        Task RestoreStock(IEnumerable<OrderItem> orderItems);

        /// <summary>
        /// Of the given products, the ones that are now at or below their minimum stock level.
        /// </summary>
        Task<List<Product>> GetProductsBelowMinimumStock(IEnumerable<int> productIds);
    }
}
