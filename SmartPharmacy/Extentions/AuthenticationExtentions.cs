using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SmartPharmacy.PL.Extentions
{
    public static class AuthenticationExtentions
    {
        public  static  IServiceCollection AddJWTAuthentication(this IServiceCollection services,
          IConfiguration configuration) {

           services.AddAuthentication(options =>
            {
                //JWT Bearer
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })

        .AddJwtBearer(options =>
        {
            //شروط التحقق من التوكن 
            options.TokenValidationParameters = new TokenValidationParameters
            {
                //تحقق من الشخص الذي أصدر الـ Token
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,     //انتهاء التوكن بالضبط عند وقت الانتهاء
                ValidateIssuerSigningKey = true,

                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
            };
        });


            return services;

        }

    }
}
