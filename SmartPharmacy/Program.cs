
using SmartPharmacy.DAL.SeedData;
using SmartPharmacy.PL.Extentions;
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

            //IdentityExtentions
            builder.Services.AddIdentityService();
            builder.Services.AddJWTAuthentication(builder.Configuration);
            builder.Services.AddHttpContextAccessor();
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

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
