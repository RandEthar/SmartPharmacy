
using SmartPharmacy.DAL.SeedData;
using SmartPharmacy.PL.Extentions;
using SmartPharmacy.PLL.Mapping;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartPharmacy
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //DataBaseExtenions
            builder.Services.AddDataBaseService(builder.Configuration);

            //ApplicationExtentions
            builder.Services.AddApplicationService();
            builder.Services.AddLocaliztionsServices();

            //IdentityExtentions
            builder.Services.AddIdentityService();
            builder.Services.AddJWTAuthentication(builder.Configuration);
            builder.Services.AddHttpContextAccessor();
            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            MapsterConfig.RegisterMappings(app.Services.GetRequiredService<IHttpContextAccessor>());
            Stripe.StripeConfiguration.ApiKey = app.Configuration["StripeSettings:SecretKey"];

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseRequestLocalization();
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var seeders = services.GetServices<ISeedData>();

                foreach (var seeder in seeders)
                {
                    await seeder.SeedData();
                }
            }
            app.Run();
        }
    }
}
