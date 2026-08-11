using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public interface IOrderService
    {
        Task<OrderResponse?> GetOrder(string userId, int orderId);
        Task<List<OrderResponse>> GetUserOrders(string userId);
        Task<bool> CancelOrder(string userId, int orderId);
        Task<PagenationResponse<OrderResponse>> GetAllOrders(OrderStatusEnum status, PagenationRequest request);
        Task<OrderResponse?> ChangeOrderState(int orderId, UpdateOrderStatusRequest request);
    }
}
