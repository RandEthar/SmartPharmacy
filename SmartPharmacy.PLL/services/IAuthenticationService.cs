using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;


namespace SmartPharmacy.PLL.services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task<RegisterResponse> Register(RegisterRequest egisterRequest);
        Task<bool> ConfirmEmail(string token, string userId);
        Task<string> GenerateAccessToken(ApplicationUser user);
        Task<ForgetPasswordResponse> RequestForgetPassword(ForgetPasswordRequest forgetPasswordRequest);
        Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest resetPasswordRequest);
    
        Task<LoginResponse> GenerateRefreshTokenAsync();
    }
}
