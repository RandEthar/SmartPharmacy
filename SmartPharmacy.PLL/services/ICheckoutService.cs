using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public interface ICheckoutService
    {
        Task<CheckoutResponse> Checkout(string userId, CheckoutRequest request);
        Task<CheckoutResponse> PayOrder(string userId, int orderId);
        Task<CheckoutResponse> ConfirmPayment(string sessionId);
    }
}
