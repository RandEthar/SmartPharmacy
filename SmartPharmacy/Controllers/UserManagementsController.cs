using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.PLL.services;
using System.Security.Claims;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class UserManagementsController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public UserManagementsController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] PagenationRequest request)
        {
            var users = await _userManagementService.GetAllUser(request);
            return Ok(users);
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userManagementService.GetUserById(userId);
            if (user == null)
            {
                return Problem(detail: $"User with id {userId} was not found.",
                    statusCode: StatusCodes.Status404NotFound);
            }
            return Ok(user);
        }

        [HttpPatch("{userId}/role")]
        public async Task<IActionResult> ChangeRole(string userId, ChangeRoleRequest request)
        {
            var result = await _userManagementService.ChangeRole(UserId, userId, request);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }

        [HttpPatch("{userId}/toggle-block")]
        public async Task<IActionResult> ToggleBlockUser(string userId)
        {
            var result = await _userManagementService.ToggleBlockUser(UserId, userId);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
