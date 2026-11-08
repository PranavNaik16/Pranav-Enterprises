using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PranavEnterprises.Application.DTOs.Task;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Web.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IProjectService _projectService;
    private readonly IEmployeeService _employeeService;

    public TasksController(
        ITaskService taskService,
        IProjectService projectService,
        IEmployeeService employeeService)
    {
        _taskService = taskService;
        _projectService = projectService;
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? projectId = null, int? employeeId = null, TaskItemStatus? status = null)
    {
        var tasks = await _taskService.GetAllTasksAsync(projectId, employeeId, status);
        var projects = await _projectService.GetAllProjectsAsync();
        var employees = await _employeeService.GetAllEmployeesAsync();

        ViewBag.Projects = new SelectList(projects, "Id", "Name", projectId);
        ViewBag.Employees = new SelectList(employees, "Id", "FullName", employeeId);
        ViewBag.ProjectId = projectId;
        ViewBag.EmployeeId = employeeId;
        ViewBag.Status = status;

        return View(tasks);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();

        return View(task);
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Create(int? projectId = null)
    {
        var projects = await _projectService.GetAllProjectsAsync();
        var employees = await _employeeService.GetAllEmployeesAsync();

        ViewBag.Projects = new SelectList(projects, "Id", "Name", projectId);
        ViewBag.Employees = new SelectList(employees, "Id", "FullName");

        return View(new CreateTaskDto
        {
            TaskCode = $"TSK-{Random.Shared.Next(100, 999)}",
            ProjectId = projectId ?? 0,
            DueDate = DateTime.Today.AddDays(7),
            EstimatedHours = 8
        });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskDto model)
    {
        if (!ModelState.IsValid)
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.Projects = new SelectList(projects, "Id", "Name", model.ProjectId);
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", model.AssignedEmployeeId);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.CreateTaskAsync(model, User.Identity?.Name, ip);

        TempData["SuccessMessage"] = $"Task '{task.Title}' created successfully.";
        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null) return NotFound();

        var projects = await _projectService.GetAllProjectsAsync();
        var employees = await _employeeService.GetAllEmployeesAsync();

        ViewBag.Projects = new SelectList(projects, "Id", "Name", task.ProjectId);
        ViewBag.Employees = new SelectList(employees, "Id", "FullName", task.AssignedEmployeeId);

        var model = new UpdateTaskDto
        {
            Id = task.Id,
            TaskCode = task.TaskCode,
            Title = task.Title,
            Description = task.Description,
            ProjectId = task.ProjectId,
            AssignedEmployeeId = task.AssignedEmployeeId,
            DueDate = task.DueDate,
            Priority = task.Priority,
            Status = task.Status,
            EstimatedHours = task.EstimatedHours,
            LoggedHours = task.LoggedHours
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTaskDto model)
    {
        if (!ModelState.IsValid)
        {
            var projects = await _projectService.GetAllProjectsAsync();
            var employees = await _employeeService.GetAllEmployeesAsync();
            ViewBag.Projects = new SelectList(projects, "Id", "Name", model.ProjectId);
            ViewBag.Employees = new SelectList(employees, "Id", "FullName", model.AssignedEmployeeId);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.UpdateTaskAsync(model, User.Identity?.Name, ip);
        if (task == null) return NotFound();

        TempData["SuccessMessage"] = $"Task '{task.Title}' updated successfully.";
        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.ManagementAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _taskService.DeleteTaskAsync(id, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = "Task removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete task.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, TaskItemStatus status)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _taskService.UpdateStatusAsync(id, status, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = $"Task status updated to {status}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
