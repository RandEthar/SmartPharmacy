using Microsoft.AspNetCore.Identity;
using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.PL.Extentions
{
    public static  class IdentityExtentions
    {
    public static IServiceCollection AddIdentityService(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;

                    options.Password.RequireDigit = true;//0-9
                    options.Password.RequireLowercase = true;//a-z
                    options.Password.RequireUppercase = true;//A-Z
                    options.Password.RequireNonAlphanumeric = true;//$#@!
                    options.Password.RequiredLength = 8;

                    options.Lockout.MaxFailedAccessAttempts = 5;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                }
            ).AddEntityFrameworkStores<ApplicationDbContext>(
             ).AddDefaultTokenProviders();
            return services;
        }
    }
}
