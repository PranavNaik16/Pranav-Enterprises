using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Application.DTOs.Employee;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;

namespace PranavEnterprises.Web.Controllers.Api;

[ApiController]
[Route("api/employees")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},Identity.Application")]
public class EmployeesApiController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesApiController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int? departmentId)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(search, departmentId);
        return Ok(new { isSuccess = true, count = employees.Count, data = employees });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null) return NotFound(new { isSuccess = false, message = "Employee not found." });

        return Ok(new { isSuccess = true, data = employee });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { isSuccess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var employee = await _employeeService.CreateEmployeeAsync(dto, User.Identity?.Name, ip);

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new { isSuccess = true, data = employee });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmployeeDto dto)
    {
        if (id != dto.Id) return BadRequest(new { isSuccess = false, message = "ID mismatch." });

        if (!ModelState.IsValid)
        {
            return BadRequest(new { isSuccess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var employee = await _employeeService.UpdateEmployeeAsync(dto, User.Identity?.Name, ip);
        if (employee == null) return NotFound(new { isSuccess = false, message = "Employee not found." });

        return Ok(new { isSuccess = true, data = employee });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.ManagementAccess)]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _employeeService.DeleteEmployeeAsync(id, User.Identity?.Name, ip);

        if (!success) return NotFound(new { isSuccess = false, message = "Employee not found or could not be deleted." });

        return Ok(new { isSuccess = true, message = "Employee deleted successfully." });
    }
}
