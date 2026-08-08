using SmartPharmacy.PLL.services;
using SmartPharmacy.DAL.SeedData;
using SmartPharmacy.DAL.Repository;

namespace SmartPharmacy.PL.Extentions
{
    public static class ApplicationExtentions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services) {
            services.AddScoped<ISeedData, RoleSeedData>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
           
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductRepository, ProductRepository>();

            services.AddScoped<IProductService, ProductService>();

            services.AddScoped<ICartRepository, CartRepository>();

            services.AddScoped<ICartService, CartService>();

            services.AddScoped<IOrderRepository, OrderRepository>();

            services.AddScoped<ICheckoutService, CheckoutService>();

            services.AddScoped<IOrderService, OrderService>();

            services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

            services.AddScoped<IPrescriptionService, PrescriptionService>();

            services.AddScoped<INotificationRepository, NotificationRepository>();

            services.AddScoped<IInventoryAlertService, InventoryAlertService>();

            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
