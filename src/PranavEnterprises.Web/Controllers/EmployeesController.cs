using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PranavEnterprises.Application.DTOs.Employee;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;

namespace PranavEnterprises.Web.Controllers;

[Authorize]
public class EmployeesController : Controller
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var employees = await _employeeService.GetAllEmployeesAsync();
        var sb = new StringBuilder();
        sb.AppendLine("EmployeeCode,FirstName,LastName,Email,Phone,Position,Department,Salary,HireDate,Status");
        foreach (var e in employees)
        {
            sb.AppendLine($"\"{e.EmployeeCode}\",\"{e.FirstName}\",\"{e.LastName}\",\"{e.Email}\",\"{e.Phone}\",\"{e.Position}\",\"{e.DepartmentName}\",{e.Salary},\"{e.HireDate:yyyy-MM-dd}\",\"{e.StatusName}\"");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"Employees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search = null, int? departmentId = null)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(search, departmentId);
        var departments = await _employeeService.GetDepartmentsAsync();

        ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);
        ViewBag.Search = search;
        ViewBag.DepartmentId = departmentId;

        return View(employees);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();

        return View(employee);
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Create()
    {
        var departments = await _employeeService.GetDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name");
        return View(new CreateEmployeeDto
        {
            EmployeeCode = $"EMP-{Random.Shared.Next(1000, 9999)}",
            HireDate = DateTime.Today
        });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEmployeeDto model)
    {
        if (!ModelState.IsValid)
        {
            var departments = await _employeeService.GetDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", model.DepartmentId);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var employee = await _employeeService.CreateEmployeeAsync(model, User.Identity?.Name, ip);

        TempData["SuccessMessage"] = $"Employee '{employee.FullName}' created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound();

        var departments = await _employeeService.GetDepartmentsAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", employee.DepartmentId);

        var model = new UpdateEmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            Position = employee.Position,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            Status = employee.Status,
            DepartmentId = employee.DepartmentId
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEmployeeDto model)
    {
        if (!ModelState.IsValid)
        {
            var departments = await _employeeService.GetDepartmentsAsync();
            ViewBag.Departments = new SelectList(departments, "Id", "Name", model.DepartmentId);
            return View(model);
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _employeeService.UpdateEmployeeAsync(model, User.Identity?.Name, ip);

        if (result == null) return NotFound();

        TempData["SuccessMessage"] = $"Employee '{result.FullName}' updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.ManagementAccess)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _employeeService.DeleteEmployeeAsync(id, User.Identity?.Name, ip);

        if (success)
        {
            TempData["SuccessMessage"] = "Employee removed successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Unable to delete employee.";
        }

        return RedirectToAction(nameof(Index));
    }
}
