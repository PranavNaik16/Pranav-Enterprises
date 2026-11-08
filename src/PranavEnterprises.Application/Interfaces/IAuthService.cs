using PranavEnterprises.Application.DTOs.Auth;

namespace PranavEnterprises.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null);
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? ipAddress = null);
    Task<UserProfileDto?> GetProfileAsync(string userId);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto);
    Task SignOutAsync();
}
