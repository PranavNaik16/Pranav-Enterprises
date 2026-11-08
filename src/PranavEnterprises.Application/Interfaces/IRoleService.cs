using PranavEnterprises.Application.DTOs.User;

namespace PranavEnterprises.Application.Interfaces;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(string id);
    Task<(bool Success, string Message)> CreateRoleAsync(CreateRoleDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<(bool Success, string Message)> DeleteRoleAsync(string id, string? currentUserName = null, string? ipAddress = null);
}
