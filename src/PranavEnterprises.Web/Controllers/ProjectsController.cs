using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PranavEnterprises.Application.DTOs.Project;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Web.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;

    public ProjectsController(IProjectService projectService, IEmployeeService employeeService)
    {
        _projectService = projectService;
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        var sb = new StringBuilder();
        sb.AppendLine("ProjectCode,Name,ClientName,Budget,StartDate,EndDate,Status,Priority,Manager,TotalTasks,CompletedTasks,ProgressPercentage");
        foreach (var p in projects)
        {
            sb.AppendLine($"\"{p.ProjectCode}\",\"{p.Name.Replace("\"", "\"\"")}\",\"{p.ClientName}\",{p.Budget},\"{p.StartDate:yyyy-MM-dd}\",\"{p.EndDate:yyyy-MM-dd}\",\"{p.StatusName}\",\"{p.PriorityName}\",\"{p.ManagerName}\",{p.TotalTasks},{p.CompletedTasks},{p.ProgressPercentage}%");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"Projects_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search = null, ProjectStatus? status = null)
    {
        var projects = await _projectService.GetAllProjectsAsync(search, status);
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(projects);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        return View(project);
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Create()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        ViewBag.Managers = new SelectList(employees, "Id", "FullName");

        return View(new CreateProjectDto
        {
            ProjectCode = $"PRJ-{DateTime.Now.Year}-{Random.Shared.Next(10, 99)}",
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(3)
        });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectDto model)
    {
        if (!ModelState.IsValid)
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.Managers = new SelectList(employees, "Id", "FullName", model.ManagerId);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var project = await _projectService.CreateProjectAsync(model, User.Identity?.Name, ip);

        TempData["SuccessMessage"] = $"Project '{project.Name}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();

        var employees = await _employeeService.GetAllEmployeesAsync();
        ViewBag.Managers = new SelectList(employees, "Id", "FullName", project.ManagerId);

        var model = new UpdateProjectDto
        {
            Id = project.Id,
            ProjectCode = project.ProjectCode,
            Name = project.Name,
            Description = project.Description,
            ClientName = project.ClientName,
            Budget = project.Budget,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status,
            Priority = project.Priority,
            ManagerId = project.ManagerId
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateProjectDto model)
    {
        if (!ModelState.IsValid)
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.Managers = new SelectList(employees, "Id", "FullName", model.ManagerId);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var project = await _projectService.UpdateProjectAsync(model, User.Identity?.Name, ip);
        if (project == null) return NotFound();

        TempData["SuccessMessage"] = $"Project '{project.Name}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.ManagementAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _projectService.DeleteProjectAsync(id, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = "Project deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete project.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ProjectStatus status)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _projectService.UpdateStatusAsync(id, status, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = $"Project status updated to {status}.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
