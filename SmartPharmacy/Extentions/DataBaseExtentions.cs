using Microsoft.EntityFrameworkCore;
using SmartPharmacy.DAL.Data;

namespace SmartPharmacy.PL.Extentions
{
    public static class DataBaseExtentions
    {
        public static IServiceCollection AddDataBaseService(this IServiceCollection services,IConfiguration configuration) {
            services.AddDbContext<ApplicationDbContext>(option =>
            {
                option.UseSqlServer(
               configuration.GetConnectionString("DefaultConnection")
           );
            });
        
            return services;
        
        }

    }
}
