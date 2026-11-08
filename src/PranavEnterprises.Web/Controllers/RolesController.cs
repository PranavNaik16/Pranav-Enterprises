using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Application.DTOs.User;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;

namespace PranavEnterprises.Web.Controllers;

[Authorize(Policy = AppPolicies.RequireSuperAdmin)]
public class RolesController : Controller
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return View(roles);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRoleDto model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid role details provided.";
            return RedirectToAction(nameof(Index));
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _roleService.CreateRoleAsync(model, User.Identity?.Name, ip);

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var (success, message) = await _roleService.DeleteRoleAsync(id, User.Identity?.Name, ip);

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
