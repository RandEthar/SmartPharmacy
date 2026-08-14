
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.Dashboard;
using SmartPharmacy.DAL.SeedData;
using SmartPharmacy.PL.Extentions;
using SmartPharmacy.PL.Middlewares;
using SmartPharmacy.PLL.Jobs;
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

            //HangfireExtentions
            builder.Services.AddHangfireService(builder.Configuration);

            //CorsExtentions
            builder.Services.AddCorsService(builder.Configuration);

            // Turns any unhandled exception into a ProblemDetails response and a log entry.
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // Request validation runs before the action does, so controllers only ever see
            // input that already satisfies the rules in the Validators folder.
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

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
            // First in the pipeline so it can catch exceptions thrown by everything after it.
            app.UseExceptionHandler();

            app.UseRequestLocalization();
            app.UseHttpsRedirection();

            app.UseStaticFiles();

            // Must precede authentication: a CORS preflight carries no token, so it would be
            // rejected with 401 before the browser ever learns the origin is allowed.
            app.UseCors(CorsExtentions.PolicyName);

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            //https://localhost:xxxx/hangfire
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireDashboardAuthorizationFilter(app.Environment) }
            });

            // Hangfire evaluates cron expressions in UTC unless told otherwise, which would fire
            // the "8 AM" report at 11:00 local time in Jordan - well after the shift starts.
            var schedulingOptions = new RecurringJobOptions
            {
                TimeZone = ResolveSchedulingTimeZone(app)
            };

            RecurringJob.AddOrUpdate<IInventoryAlertJob>(
                "inventory-alert-check",
                job => job.Run(),
                Cron.Daily(8),
                schedulingOptions);

            // Hourly, because every stale order is stock sitting unavailable to other customers.
            RecurringJob.AddOrUpdate<IExpireStaleOrdersJob>(
                "expire-stale-orders",
                job => job.Run(),
                Cron.Hourly(),
                schedulingOptions);

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

        /// <summary>
        /// Time zone the recurring jobs are scheduled in. Read from configuration rather than
        /// hardcoded so a deployment in another region does not need a code change, and falls
        /// back to UTC instead of crashing on startup if the id is not recognised.
        /// </summary>
        private static TimeZoneInfo ResolveSchedulingTimeZone(WebApplication app)
        {
            var timeZoneId = app.Configuration["Scheduling:TimeZone"];

            if (string.IsNullOrWhiteSpace(timeZoneId))
                return TimeZoneInfo.Utc;

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                app.Logger.LogWarning(ex,
                    "Unknown Scheduling:TimeZone '{TimeZoneId}'. Recurring jobs will run in UTC.",
                    timeZoneId);

                return TimeZoneInfo.Utc;
            }
        }
    }
}
