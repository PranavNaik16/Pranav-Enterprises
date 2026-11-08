using Microsoft.AspNetCore.Identity;
using PranavEnterprises.Application.DTOs.Auth;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Entities;

namespace PranavEnterprises.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IActivityLogService _activityLogService;
    private readonly IEmailSenderService _emailSenderService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        JwtTokenService jwtTokenService,
        IActivityLogService activityLogService,
        IEmailSenderService emailSenderService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _activityLogService = activityLogService;
        _emailSenderService = emailSenderService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string? ipAddress = null)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid email or password",
                Errors = new List<string> { "Invalid credentials provided." }
            };
        }

        if (!user.IsApproved)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Your account is pending role approval by Super Admin (pranavnaik355@gmail.com). You will be notified once approved.",
                Errors = new List<string> { "Account approval pending." }
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Account is deactivated. Please contact your system administrator.",
                Errors = new List<string> { "Account deactivated." }
            };
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, dto.Password, dto.RememberMe, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            await _activityLogService.LogAsync(user.Id, user.Email!, "Unknown", "LoginLocked", "Auth", user.Id, "User account locked due to multiple failed login attempts.", ipAddress);
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User account is locked due to multiple failed login attempts.",
                Errors = new List<string> { "Account locked out." }
            };
        }

        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "Invalid email or password",
                Errors = new List<string> { "Invalid credentials." }
            };
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? AppRoles.Employee;

        var (token, expiration) = _jwtTokenService.GenerateToken(user, roles);

        await _activityLogService.LogAsync(user.Id, user.Email!, primaryRole, "Login", "Auth", user.Id, "Successful authentication into system.", ipAddress);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = "Authentication successful",
            Token = token,
            Expiration = expiration,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = primaryRole
        };
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto, string? ipAddress = null)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "A user with this email address already exists.",
                Errors = new List<string> { "Email is already registered." }
            };
        }

        var targetRole = AppRoles.All.Contains(dto.Role) ? dto.Role : AppRoles.Employee;

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            JobTitle = dto.JobTitle ?? "Staff Associate",
            DepartmentName = dto.DepartmentName ?? "General Operations",
            EmailConfirmed = true,
            IsActive = false,     // Requires Super Admin approval
            IsApproved = false,   // Requires Super Admin approval
            RequestedRole = targetRole,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = "User registration failed.",
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        // Initially assign Employee role
        await _userManager.AddToRoleAsync(user, AppRoles.Employee);

        // Notify Super Admin (pranavnaik355@gmail.com)
        const string adminEmail = "pranavnaik355@gmail.com";
        var approveUrl = "http://localhost:5000/Users/Pending";

        await _emailSenderService.SendRegistrationNotificationAsync(
            adminEmail,
            user.FullName,
            user.Email,
            targetRole,
            user.JobTitle,
            user.DepartmentName,
            approveUrl);

        await _activityLogService.LogAsync(
            user.Id,
            user.Email,
            "Guest",
            "Register",
            "Auth",
            user.Id,
            $"New registration '{user.FullName}' ({user.Email}) requesting role '{targetRole}'. Email dispatch sent to {adminEmail} for approval.",
            ipAddress);

        return new AuthResponseDto
        {
            IsSuccess = true,
            Message = $"Registration submitted successfully! An approval dispatch has been sent to Super Admin ({adminEmail}). You can log in once approved.",
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = targetRole
        };
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? AppRoles.Employee,
            JobTitle = user.JobTitle,
            DepartmentName = user.DepartmentName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            await _activityLogService.LogAsync(user.Id, user.Email!, roles.FirstOrDefault() ?? "User", "ChangePassword", "Auth", user.Id, "Password updated successfully.");
            return true;
        }

        return false;
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
