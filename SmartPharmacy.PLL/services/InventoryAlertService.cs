using Mapster;
using Microsoft.Extensions.Configuration;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;

namespace SmartPharmacy.PLL.services
{
    public class InventoryAlertService : IInventoryAlertService
    {
        private const int DefaultExpiryWarningDays = 30;

        private readonly IProductRepository _productRepository;
        private readonly int _expiryWarningDays;

        public InventoryAlertService(IProductRepository productRepository, IConfiguration config)
        {
            _productRepository = productRepository;

            _expiryWarningDays =
                int.TryParse(config["InventoryAlerts:ExpiryWarningDays"], out var days)
                    ? days
                    : DefaultExpiryWarningDays;
        }


        public async Task<List<InventoryAlertResponse>> GetLowStockProducts()
        {
            var products = await _productRepository.GetAllAsync(
                p => p.entitystate == Entitystate.Active
                     && p.StockQuantity <= p.MinimumStock,
                new[] { nameof(Product.ProductTranslations) });

            return products.Adapt<List<InventoryAlertResponse>>();
        }


        public async Task<List<InventoryAlertResponse>> GetNearExpiryProducts()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var warningLimit = today.AddDays(_expiryWarningDays);

            var products = await _productRepository.GetAllAsync(
                p => p.entitystate == Entitystate.Active
                     && p.ExpiryDate >= today
                     && p.ExpiryDate <= warningLimit,
                new[] { nameof(Product.ProductTranslations) });

            return products
                .OrderBy(p => p.ExpiryDate)
                .ToList()
                .Adapt<List<InventoryAlertResponse>>();
        }


        public async Task<List<InventoryAlertResponse>> GetExpiredProducts()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var products = await _productRepository.GetAllAsync(
                p => p.entitystate == Entitystate.Active
                     && p.ExpiryDate < today,
                new[] { nameof(Product.ProductTranslations) });

            return products
                .OrderBy(p => p.ExpiryDate)
                .ToList()
                .Adapt<List<InventoryAlertResponse>>();
        }
    }
}
