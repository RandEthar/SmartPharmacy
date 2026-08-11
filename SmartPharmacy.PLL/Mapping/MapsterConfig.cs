using Mapster;
using Microsoft.AspNetCore.Http;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using System.Globalization;
using System.Linq;

namespace SmartPharmacy.PLL.Mapping
{
    public static class MapsterConfig
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void RegisterMappings(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            TypeAdapterConfig<Category, CategoryResponse>.NewConfig()
                .Map(dest => dest.Name, src => src.CategoryTranslations
                    .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                    .Select(t => t.Name)
                    .FirstOrDefault())
                .Map(dest => dest.Image, src => BuildImageUrl(src.Image));

            TypeAdapterConfig<Product, ProductResponse>.NewConfig()
               .Map(dest => dest.Name, src => src.ProductTranslations
                   .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                   .Select(t => t.Name)
                   .FirstOrDefault())
               .Map(dest => dest.Description, src => src.ProductTranslations
                   .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                   .Select(t => t.Description)
                   .FirstOrDefault())
               .Map(dest => dest.Image, src => BuildImageUrl(src.Image))
               .Map(dest => dest.SubImages, src => src.ProductImages.Select(img => BuildImageUrl(img.ImageUrl)).ToList());

            TypeAdapterConfig<CartItem, CartItemResponse>.NewConfig()
               .Map(dest => dest.ProductName, src => src.Product.ProductTranslations
                   .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                   .Select(t => t.Name)
                   .FirstOrDefault())
               .Map(dest => dest.ProductImage, src => BuildImageUrl(src.Product.Image))
               .Map(dest => dest.Price, src => src.Product.Price);

            TypeAdapterConfig<OrderItem, OrderItemResponse>.NewConfig()
               .Map(dest => dest.ProductName, src => src.Product.ProductTranslations
                   .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                   .Select(t => t.Name)
                   .FirstOrDefault());

            TypeAdapterConfig<Order, OrderResponse>.NewConfig()
               .Map(dest => dest.Items, src => src.OrderItems);

            TypeAdapterConfig<Prescription, PrescriptionResponse>.NewConfig()
               .Map(dest => dest.ImageUrl, src => BuildImageUrl(src.ImageUrl));

            TypeAdapterConfig<Product, InventoryAlertResponse>.NewConfig()
               .Map(dest => dest.ProductId, src => src.Id)
               .Map(dest => dest.Name, src => src.ProductTranslations
                   .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                   .Select(t => t.Name)
                   .FirstOrDefault())
               .Map(dest => dest.Image, src => BuildImageUrl(src.Image))
               .Map(dest => dest.DaysUntilExpiry,
                    src => src.ExpiryDate.DayNumber - DateOnly.FromDateTime(DateTime.UtcNow).DayNumber);

            TypeAdapterConfig<NotificationProduct, NotificationProductResponse>.NewConfig()
               .Map(dest => dest.Name, src => src.Product.ProductTranslations
                   .Where(t => t.Language == CultureInfo.CurrentCulture.Name)
                   .Select(t => t.Name)
                   .FirstOrDefault())
               .Map(dest => dest.StockQuantity, src => src.Product.StockQuantity)
               .Map(dest => dest.MinimumStock, src => src.Product.MinimumStock)
               .Map(dest => dest.ExpiryDate, src => src.Product.ExpiryDate);

            TypeAdapterConfig<Notification, NotificationResponse>.NewConfig()
               .Map(dest => dest.Products, src => src.NotificationProducts);

            TypeAdapterConfig<ApplicationUser, UserResponse>.NewConfig()
               .Map(dest => dest.IsBlocked,
                    src => src.LockoutEnd.HasValue && src.LockoutEnd > DateTimeOffset.UtcNow);

            TypeAdapterConfig<ApplicationUser, UserDetailResponse>.NewConfig()
               .Map(dest => dest.IsBlocked,
                    src => src.LockoutEnd.HasValue && src.LockoutEnd > DateTimeOffset.UtcNow);
        }

        private static string BuildImageUrl(string image)
        {
            if (string.IsNullOrEmpty(image)) return null;

            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return $"/Images/{image}";

            return $"{request.Scheme}://{request.Host}/Images/{image}";
        }
    }
}
