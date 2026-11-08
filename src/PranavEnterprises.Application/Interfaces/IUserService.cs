using PranavEnterprises.Application.DTOs.User;

namespace PranavEnterprises.Application.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync(string? search = null);
    Task<List<UserDto>> GetPendingApprovalsAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<(bool Success, string Message)> ApproveUserRoleAsync(string userId, string approvedRole, string currentUserName, string? ipAddress = null);
    Task<(bool Success, string Message)> RejectUserAsync(string userId, string currentUserName, string? ipAddress = null);
    Task<(bool Success, string Message)> ToggleUserActiveAsync(string id, string? currentUserName = null, string? ipAddress = null);
    Task<(bool Success, string Message)> DeleteUserAsync(string id, string? currentUserName = null, string? ipAddress = null);
}
