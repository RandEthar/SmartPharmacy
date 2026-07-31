
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.PLL.services;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationsController : ControllerBase
    {private readonly IAuthenticationService _authenticationService;
        public AuthenticationsController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
         public   async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authenticationService.Register(request);
            return Ok(result);

        }
        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestResetPassword(ForgetPasswordRequest request)
        {
            var result = await _authenticationService.RequestForgetPassword(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _authenticationService.Login(request);
            return Ok(result);

        }
        [HttpGet("confirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token,string userId)
        {
            var result = await _authenticationService.ConfirmEmail(token, userId);
            return Ok(result);

        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResertPassword(ResetPasswordRequest request)
        {
            var result = await _authenticationService.ResetPassword(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _authenticationService.GenerateRefreshTokenAsync();
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);

        }
    }
}
