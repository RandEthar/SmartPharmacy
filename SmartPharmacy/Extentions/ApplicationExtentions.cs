using SmartPharmacy.PLL.services;
using SmartPharmacy.DAL.SeedData;

namespace SmartPharmacy.PL.Extentions
{
    public static class ApplicationExtentions
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services) {
            services.AddScoped<ISeedData, RoleSeedData>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IEmailSender, EmailSender>();
            return services;
        }
    }
}
