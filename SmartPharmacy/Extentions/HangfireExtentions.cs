using Hangfire;

namespace SmartPharmacy.PL.Extentions
{
    public static class HangfireExtentions
    {
        public static IServiceCollection AddHangfireService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetRequiredConnectionString("DefaultConnection")));

            services.AddHangfireServer();

            return services;
        }
    }
}
