using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public interface ICartService
    {
        Task<CartResponse> GetCart(string userId);
        Task<CartItemResponse> AddToCart(string userId, CartItemRequest request);
        Task<CartItemResponse> UpdateQuantity(string userId, int productId, UpdateCartItemRequest request);
        Task<bool> RemoveFromCart(string userId, int productId);
        Task<bool> ClearCart(string userId);
    }
}
