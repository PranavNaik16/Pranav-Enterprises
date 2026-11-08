using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.User;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Entities;

namespace PranavEnterprises.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IActivityLogService _activityLogService;
    private readonly IEmailSenderService _emailSenderService;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IActivityLogService activityLogService,
        IEmailSenderService emailSenderService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _activityLogService = activityLogService;
        _emailSenderService = emailSenderService;
    }

    public async Task<List<UserDto>> GetAllUsersAsync(string? search = null)
    {
        var usersQuery = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            usersQuery = usersQuery.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.JobTitle != null && u.JobTitle.ToLower().Contains(s)));
        }

        var users = await usersQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();
        var result = new List<UserDto>();

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "None",
                JobTitle = u.JobTitle,
                DepartmentName = u.DepartmentName,
                IsActive = u.IsActive,
                IsApproved = u.IsApproved,
                RequestedRole = u.RequestedRole,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            });
        }

        return result;
    }

    public async Task<List<UserDto>> GetPendingApprovalsAsync()
    {
        var users = await _userManager.Users
            .Where(u => !u.IsApproved)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var result = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "None",
                JobTitle = u.JobTitle,
                DepartmentName = u.DepartmentName,
                IsActive = u.IsActive,
                IsApproved = u.IsApproved,
                RequestedRole = u.RequestedRole ?? "Employee",
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            });
        }

        return result;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? "None",
            JobTitle = user.JobTitle,
            DepartmentName = user.DepartmentName,
            IsActive = user.IsActive,
            IsApproved = user.IsApproved,
            RequestedRole = user.RequestedRole,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<(bool Success, string Message)> ApproveUserRoleAsync(
        string userId,
        string approvedRole,
        string currentUserName,
        string? ipAddress = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found.");

        user.IsApproved = true;
        user.IsActive = true;
        user.ApprovedAt = DateTime.UtcNow;
        user.ApprovedBy = currentUserName;

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        var targetRole = AppRoles.All.Contains(approvedRole) ? approvedRole : AppRoles.Employee;
        if (!await _roleManager.RoleExistsAsync(targetRole))
        {
            await _roleManager.CreateAsync(new ApplicationRole(targetRole));
        }

        await _userManager.AddToRoleAsync(user, targetRole);
        await _userManager.UpdateAsync(user);

        await _activityLogService.LogAsync(
            user.Id,
            currentUserName,
            "SuperAdmin",
            "ApproveRole",
            "User",
            user.Id,
            $"Super Admin ({currentUserName}) approved user '{user.FullName}' ({user.Email}) with role '{targetRole}'.",
            ipAddress);

        // Notify user via email
        var subject = "Your Pranav Enterprise Account Has Been Approved!";
        var body = $"<p>Hello {user.FullName},</p><p>Your account registration has been approved by Super Admin <strong>Pranav Naik</strong>.</p><p>Your approved role is: <strong>{targetRole}</strong>.</p><p>You can now sign in at <a href='http://localhost:5000/Account/Login'>Pranav Enterprise Portal</a>.</p>";
        await _emailSenderService.SendEmailAsync(user.Email!, subject, body);

        return (true, $"User '{user.FullName}' approved successfully with role '{targetRole}'.");
    }

    public async Task<(bool Success, string Message)> RejectUserAsync(
        string userId,
        string currentUserName,
        string? ipAddress = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found.");

        await _activityLogService.LogAsync(
            user.Id,
            currentUserName,
            "SuperAdmin",
            "RejectUser",
            "User",
            user.Id,
            $"Registration rejected for user '{user.FullName}' ({user.Email}).",
            ipAddress);

        await _userManager.DeleteAsync(user);
        return (true, $"Registration for '{user.FullName}' was rejected and removed.");
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
        {
            return (false, "A user with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            JobTitle = dto.JobTitle,
            DepartmentName = dto.DepartmentName,
            EmailConfirmed = true,
            IsActive = true,
            IsApproved = true, // Admin-created users are pre-approved
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await _roleManager.RoleExistsAsync(dto.Role))
        {
            await _roleManager.CreateAsync(new ApplicationRole(dto.Role));
        }

        await _userManager.AddToRoleAsync(user, dto.Role);

        await _activityLogService.LogAsync(
            user.Id,
            currentUserName ?? "Administrator",
            "SuperAdmin",
            "Create",
            "User",
            user.Id,
            $"Provisioned user account '{user.FullName}' ({user.Email}) with role '{dto.Role}'.",
            ipAddress);

        return (true, "User created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null) return (false, "User not found.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.JobTitle = dto.JobTitle;
        user.DepartmentName = dto.DepartmentName;
        user.IsActive = dto.IsActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        if (!await _roleManager.RoleExistsAsync(dto.Role))
        {
            await _roleManager.CreateAsync(new ApplicationRole(dto.Role));
        }
        await _userManager.AddToRoleAsync(user, dto.Role);

        await _activityLogService.LogAsync(
            user.Id,
            currentUserName ?? "Administrator",
            "SuperAdmin",
            "Update",
            "User",
            user.Id,
            $"Updated user profile and role assignment for '{user.FullName}' to '{dto.Role}'.",
            ipAddress);

        return (true, "User updated successfully.");
    }

    public async Task<(bool Success, string Message)> ToggleUserActiveAsync(string id, string? currentUserName = null, string? ipAddress = null)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found.");

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        var status = user.IsActive ? "Activated" : "Deactivated";

        await _activityLogService.LogAsync(
            user.Id,
            currentUserName ?? "Administrator",
            "SuperAdmin",
            "StatusChange",
            "User",
            user.Id,
            $"{status} user account for '{user.FullName}' ({user.Email}).",
            ipAddress);

        return (true, $"User account successfully {status.ToLower()}.");
    }

    public async Task<(bool Success, string Message)> DeleteUserAsync(string id, string? currentUserName = null, string? ipAddress = null)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return (false, "User not found.");

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _activityLogService.LogAsync(
            id,
            currentUserName ?? "Administrator",
            "SuperAdmin",
            "Delete",
            "User",
            id,
            $"Deleted user account '{user.FullName}' ({user.Email}).",
            ipAddress);

        return (true, "User deleted successfully.");
    }
}
