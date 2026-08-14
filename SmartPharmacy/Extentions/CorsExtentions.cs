namespace SmartPharmacy.PL.Extentions
{
    public static class CorsExtentions
    {
        public const string PolicyName = "FrontendClients";

        /// <summary>
        /// Browsers block a web frontend from reading this API's responses unless the API says
        /// which origins are allowed. Mobile apps are unaffected - CORS is a browser rule only.
        /// </summary>
        public static IServiceCollection AddCorsService(
            this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    if (allowedOrigins.Length == 0)
                    {
                        // No origins configured: allow reads from anywhere but never credentials,
                        // since AllowAnyOrigin and AllowCredentials cannot legally be combined.
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                        return;
                    }

                    // AllowCredentials is required because the refresh token travels in an
                    // HttpOnly cookie; without it /refresh-token fails from a browser.
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            return services;
        }
    }
}
