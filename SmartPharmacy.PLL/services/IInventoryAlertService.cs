using SmartPharmacy.DAL.DTO.Response;

namespace SmartPharmacy.PLL.services
{
    public interface IInventoryAlertService
    {
        Task<List<InventoryAlertResponse>> GetLowStockProducts();
        Task<List<InventoryAlertResponse>> GetNearExpiryProducts();
        Task<List<InventoryAlertResponse>> GetExpiredProducts();
    }
}
