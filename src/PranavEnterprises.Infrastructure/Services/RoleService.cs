using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.User;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Infrastructure.Data;

namespace PranavEnterprises.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public RoleService(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IActivityLogService activityLogService)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
        var result = new List<RoleDto>();

        foreach (var r in roles)
        {
            var count = await _context.UserRoles.CountAsync(ur => ur.RoleId == r.Id);
            result.Add(new RoleDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                Description = r.Description,
                UsersCount = count
            });
        }

        return result;
    }

    public async Task<RoleDto?> GetRoleByIdAsync(string id)
    {
        var r = await _roleManager.FindByIdAsync(id);
        if (r == null) return null;

        var count = await _context.UserRoles.CountAsync(ur => ur.RoleId == r.Id);
        return new RoleDto
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Description = r.Description,
            UsersCount = count
        };
    }

    public async Task<(bool Success, string Message)> CreateRoleAsync(CreateRoleDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
        {
            return (false, "Role with this name already exists.");
        }

        var role = new ApplicationRole(dto.Name, dto.Description);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _activityLogService.LogAsync(
            role.Id,
            currentUserName ?? "Administrator",
            "SuperAdmin",
            "Create",
            "Role",
            role.Id,
            $"Created new system role '{role.Name}'.",
            ipAddress);

        return (true, "Role created successfully.");
    }

    public async Task<(bool Success, string Message)> DeleteRoleAsync(string id, string? currentUserName = null, string? ipAddress = null)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null) return (false, "Role not found.");

        if (AppRoles.All.Contains(role.Name))
        {
            return (false, "System core roles cannot be deleted.");
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            return (false, $"Cannot delete role '{role.Name}' because it has {usersInRole.Count} assigned user(s).");
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        await _activityLogService.LogAsync(
            id,
            currentUserName ?? "Administrator",
            "SuperAdmin",
            "Delete",
            "Role",
            id,
            $"Deleted custom role '{role.Name}'.",
            ipAddress);

        return (true, "Role deleted successfully.");
    }
}
