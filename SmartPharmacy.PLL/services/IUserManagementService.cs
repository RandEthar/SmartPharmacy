using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;

namespace SmartPharmacy.PLL.services
{
    public interface IUserManagementService
    {
        Task<PagenationResponse<UserResponse>> GetAllUser(PagenationRequest request);
        Task<UserDetailResponse> GetUserById(string userId);
        Task<OperationResponse> ChangeRole(string currentUserId, string targetUserId, ChangeRoleRequest newRole);
        Task<OperationResponse> ToggleBlockUser(string currentUserId, string targetUserId);
    }
}
