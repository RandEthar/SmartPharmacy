using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartPharmacy.PLL.services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;


        public AuthenticationService(
            IEmailSender emailSender,
            IConfiguration config,
            UserManager<ApplicationUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }


        public async Task<bool> ConfirmEmail(string token, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);

            return result.Succeeded;
        }


        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);

            if (user == null)
            {
                return new LoginResponse
                {
                    Message = "Invalid email or password.",
                    Success = false
                };
            }


            // CheckPasswordAsync ignores lockout on its own, so a blocked user would keep
            // logging in unless it is checked explicitly here.
            if (await _userManager.IsLockedOutAsync(user))
            {
                return new LoginResponse
                {
                    Message = "This account is blocked. Please contact support.",
                    Success = false
                };
            }


            var passwordValid =
                await _userManager.CheckPasswordAsync(user, loginRequest.Password);


            if (!passwordValid)
            {
                // Without this the configured 5-attempt lockout policy never actually triggers.
                await _userManager.AccessFailedAsync(user);

                return new LoginResponse
                {
                    Message = "Invalid email or password.",
                    Success = false
                };
            }


            await _userManager.ResetAccessFailedCountAsync(user);


            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return new LoginResponse
                {
                    Message = "Please confirm your email before login.",
                    Success = false
                };
            }
            var refreshToken=await  GenerateRefreshTokenAsync(user);
            setRefreshTokenInCookie(refreshToken);
     
            return new LoginResponse
            {
                Message = "Login successful.",
                Success = true,
                AccessToken = await GenerateAccessToken(user)
            };
        }



        public async Task<RegisterResponse> Register(RegisterRequest registerRequest)
        {
            var user = registerRequest.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(
                user,
                registerRequest.Password
            );


            if (result.Succeeded)
            {

                await _userManager.AddToRoleAsync(user, Roles.Patient);


                var token =
                    await _userManager.GenerateEmailConfirmationTokenAsync(user);

                //رمز مؤقت لتأكيد الإيميل
                token = Uri.EscapeDataString(token);
                //scheme =>https or http, host => localhost:port or domain

                var confirmEmailUrl =
                $"{_httpContextAccessor.HttpContext.Request.Scheme}://" +
                $"{_httpContextAccessor.HttpContext.Request.Host}" +
                $"/api/Authentications/ConfirmEmail?token={token}&userId={user.Id}";


                await _emailSender.SendEmailAsync(
                    user.Email,
                    "Confirm Your Email",
                    $@"
                    <h1>Welcome to Smart Pharmacy</h1>
                    <p>Please confirm your email:</p>
                    <a href='{confirmEmailUrl}'>
                        Confirm Email
                    </a>
                    "
                );


                return new RegisterResponse
                {
                    Message = "User created successfully.",
                    Success = true
                };
            }


            return new RegisterResponse
            {
                Message = "User creation failed.",
                Success = false,
                Errors = result.Errors
                    .Select(e => e.Description)
                    .ToList()
            };
        }



        public async Task<string> GenerateAccessToken(ApplicationUser user)
        {
            //لتوقيع الـ Token
            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]!)
                );


            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256
                );

            //معلومات منحطها داخل الـ Token
            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email!
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.UserName!
                )
            };

            //بدون هدول الـ claims أي [Authorize(Roles = ...)] رح ترفض كل المستخدمين
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }


            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"], //مين انشاء هاد التوكن 
                audience: _config["Jwt:Audience"],//مين مسموح له يستخدم هاد التوكن
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );


            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }



        public async Task<ForgetPasswordResponse> RequestForgetPassword(
            ForgetPasswordRequest forgetPasswordRequest)
        {

            var user =
                await _userManager.FindByEmailAsync(
                    forgetPasswordRequest.Email
                );


            if (user == null)
            {
                return new ForgetPasswordResponse
                {
                    Message = "No account found with this email.",
                    Success = false
                };
            }


            var random = new Random();

            var code =
                random.Next(1000, 9999)
                .ToString();


            user.CodeResetPassword = code;

            user.CodeResetPasswordExpiration =
                DateTime.UtcNow.AddMinutes(15);


            await _userManager.UpdateAsync(user);



            await _emailSender.SendEmailAsync(
                user.Email,
                "Password Reset Request",
                $@"
                <h1>Password Reset</h1>
                <p>Your reset code is:</p>
                <h2>{code}</h2>
                <p>This code expires after 15 minutes.</p>
                "
            );


            return new ForgetPasswordResponse
            {
                Message = "Password reset code sent successfully.",
                Success = true
            };
        }



        public async Task<ResetPasswordResponse> ResetPassword(
            ResetPasswordRequest resetPasswordRequest)
        {

            var user =
                await _userManager.FindByEmailAsync(
                    resetPasswordRequest.Email
                );


            if (user == null)
            {
                return new ResetPasswordResponse
                {
                    Message = "No account found with this email.",
                    Success = false
                };
            }



            if (user.CodeResetPassword != resetPasswordRequest.Code)
            {
                return new ResetPasswordResponse
                {
                    Message = "Invalid reset code.",
                    Success = false
                };
            }



            if (user.CodeResetPasswordExpiration < DateTime.UtcNow)
            {
                return new ResetPasswordResponse
                {
                    Message = "Reset code has expired.",
                    Success = false
                };
            }



            var removePassword =
                await _userManager.RemovePasswordAsync(user);


            if (!removePassword.Succeeded)
            {
                return new ResetPasswordResponse
                {
                    Message = "Password reset failed.",
                    Success = false
                };
            }



            var addPassword =
                await _userManager.AddPasswordAsync(
                    user,
                    resetPasswordRequest.NewPassword
                );


            if (!addPassword.Succeeded)
            {
                return new ResetPasswordResponse
                {
                    Message = "Password reset failed.",
                    Success = false
                };
            }



            user.CodeResetPassword = null;
            user.CodeResetPasswordExpiration = null;


            await _userManager.UpdateAsync(user);



            return new ResetPasswordResponse
            {
                Message = "Password changed successfully.",
                Success = true
            };
        }



      private async Task<string> GenerateRefreshTokenAsync(ApplicationUser user)
        {
            string refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(15);
            await _userManager.UpdateAsync(user);
            return refreshToken;
        }
       void setRefreshTokenInCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,//true in production
                SameSite = SameSiteMode.None,//strict in production,
                Expires = DateTime.UtcNow.AddDays(15)


            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    

        public async Task<LoginResponse> GenerateRefreshTokenAsync()
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"];
            if (refreshToken is null)
            {
                return new LoginResponse { Message = "No refresh token provided", Success = false };
            }
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if (user is null || user.RefreshTokenExpiration < DateTime.UtcNow)
            {
                return new LoginResponse { Message = "Invalid or expired refresh token", Success = false };
            }

            // Otherwise a blocked user keeps minting fresh access tokens from an old cookie.
            if (await _userManager.IsLockedOutAsync(user))
            {
                return new LoginResponse { Message = "This account is blocked. Please contact support.", Success = false };
            }
            var newRefreshToken = await GenerateRefreshTokenAsync(user);
            setRefreshTokenInCookie(newRefreshToken);
            var newAccessToken = await GenerateAccessToken(user);

            return new LoginResponse
            {
                Message = "Token refreshed successfully",
                Success = true,
                AccessToken = newAccessToken


            };
        }
    }
}