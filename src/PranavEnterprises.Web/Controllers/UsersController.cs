using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PranavEnterprises.Application.DTOs.User;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;

namespace PranavEnterprises.Web.Controllers;

[Authorize(Policy = AppPolicies.ManagementAccess)]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;

    public UsersController(IUserService userService, IRoleService roleService)
    {
        _userService = userService;
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search = null)
    {
        var users = await _userService.GetAllUsersAsync(search);
        var pending = await _userService.GetPendingApprovalsAsync();
        ViewBag.Search = search;
        ViewBag.PendingCount = pending.Count;
        return View(users);
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.RequireSuperAdmin)]
    public async Task<IActionResult> Pending()
    {
        var pendingUsers = await _userService.GetPendingApprovalsAsync();
        var roles = await _roleService.GetAllRolesAsync();
        ViewBag.Roles = roles;
        return View(pendingUsers);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireSuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRole(string id, string approvedRole)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _userService.ApproveUserRoleAsync(id, approvedRole, User.Identity?.Name ?? "Pranav Naik", ip);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Pending));
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireSuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(string id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _userService.RejectUserAsync(id, User.Identity?.Name ?? "Pranav Naik", ip);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Pending));
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.RequireSuperAdmin)]
    public async Task<IActionResult> Create()
    {
        var roles = await _roleService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(roles, "Name", "Name");
        return View(new CreateUserDto());
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireSuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _roleService.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles, "Name", "Name", model.Role);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _userService.CreateUserAsync(model, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        var roleList = await _roleService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(roleList, "Name", "Name", model.Role);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _roleService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(roles, "Name", "Name", user.Role);

        var model = new UpdateUserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            JobTitle = user.JobTitle,
            DepartmentName = user.DepartmentName,
            IsActive = user.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateUserDto model)
    {
        if (!ModelState.IsValid)
        {
            var roles = await _roleService.GetAllRolesAsync();
            ViewBag.Roles = new SelectList(roles, "Name", "Name", model.Role);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _userService.UpdateUserAsync(model, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError(string.Empty, message);
        var roleList = await _roleService.GetAllRolesAsync();
        ViewBag.Roles = new SelectList(roleList, "Name", "Name", model.Role);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _userService.ToggleUserActiveAsync(id, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireSuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _userService.DeleteUserAsync(id, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToAction(nameof(Index));
    }
}
