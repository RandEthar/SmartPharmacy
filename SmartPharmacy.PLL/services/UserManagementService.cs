using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Extentions;
using SmartPharmacy.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class UserManagementService: IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserManagementService(UserManager<ApplicationUser> userManager,
             RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public async Task<OperationResponse> ChangeRole(string currentUserId, string targetUserId, ChangeRoleRequest newRole)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (user == null)
            {
                return OperationResponse.Fail("User not found.");
            }

            // An admin demoting themselves can leave the system with no way back in.
            if (string.Equals(currentUserId, targetUserId, StringComparison.Ordinal))
            {
                return OperationResponse.Fail("You cannot change your own role.");
            }

            // Checked *before* anything is removed - the old code deleted every role first and
            // then failed on an unknown role name, leaving the user with no role at all.
            if (!await _roleManager.RoleExistsAsync(newRole.Role))
            {
                return OperationResponse.Fail($"Role '{newRole.Role}' does not exist.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains(newRole.Role) && currentRoles.Count == 1)
            {
                return OperationResponse.Ok("User already has this role.");
            }

            if (currentRoles.Contains(Roles.Admin) && newRole.Role != Roles.Admin)
            {
                var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
                if (admins.Count <= 1)
                {
                    return OperationResponse.Fail("Cannot demote the last remaining Admin.");
                }
            }

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                return OperationResponse.Fail(Describe(removeResult));
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole.Role);
            if (!addResult.Succeeded)
            {
                // Put the user back where they were rather than leaving them role-less.
                await _userManager.AddToRolesAsync(user, currentRoles);
                return OperationResponse.Fail(Describe(addResult));
            }

            return OperationResponse.Ok($"Role changed to '{newRole.Role}'.");
        }

        public async Task<PagenationResponse<UserResponse>> GetAllUser(PagenationRequest request)
        {
            var query = _userManager.Users
                .OrderBy(u => u.FullName)
                .ThenBy(u => u.Id);

            var users = await query.ApplyPagenation(request.Page, request.Limit);

            return new PagenationResponse<UserResponse>
            {
                Data = users.Data.Adapt<List<UserResponse>>(),
                TotalCount = users.TotalCount,
                Page = users.Page,
                Limit = users.Limit
            };
        }

        public async Task<UserDetailResponse> GetUserById(string userId)
        {
           var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return null;
            }
            var userDetail = user.Adapt<UserDetailResponse>();
            var roles = await _userManager.GetRolesAsync(user);
            userDetail.Roles = roles.ToList();
            return userDetail;
        }

        public async Task<OperationResponse> ToggleBlockUser(string currentUserId, string targetUserId)
        {
            var user = await _userManager.FindByIdAsync(targetUserId);
            if (user == null)
            {
                return OperationResponse.Fail("User not found.");
            }

            if (string.Equals(currentUserId, targetUserId, StringComparison.Ordinal))
            {
                return OperationResponse.Fail("You cannot block yourself.");
            }

            var isBlocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            if (isBlocked)
            {
                var unblock = await _userManager.SetLockoutEndDateAsync(user, null);
                return unblock.Succeeded
                    ? OperationResponse.Ok("User unblocked.")
                    : OperationResponse.Fail(Describe(unblock));
            }

            // Lockout must be enabled on the account, otherwise Identity ignores the end date.
            var enable = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!enable.Succeeded)
            {
                return OperationResponse.Fail(Describe(enable));
            }

            // DateTimeOffset.MaxValue is the Identity convention for an indefinite block;
            // it stays blocked until an admin explicitly unblocks.
            var block = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            if (!block.Succeeded)
            {
                return OperationResponse.Fail(Describe(block));
            }

            // Any refresh token still in the wild would otherwise keep minting access tokens.
            user.RefreshToken = null;
            user.RefreshTokenExpiration = null;
            await _userManager.UpdateAsync(user);

            return OperationResponse.Ok("User blocked.");
        }

        private static string Describe(IdentityResult result) =>
            string.Join("; ", result.Errors.Select(e => e.Description));
    }
}
