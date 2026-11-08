using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Application.DTOs.Task;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Web.Controllers.Api;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},Identity.Application")]
public class TasksApiController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksApiController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? projectId, [FromQuery] int? employeeId, [FromQuery] TaskItemStatus? status)
    {
        var tasks = await _taskService.GetAllTasksAsync(projectId, employeeId, status);
        return Ok(new { isSuccess = true, count = tasks.Count, data = tasks });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null) return NotFound(new { isSuccess = false, message = "Task not found." });

        return Ok(new { isSuccess = true, data = task });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { isSuccess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.CreateTaskAsync(dto, User.Identity?.Name, ip);

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, new { isSuccess = true, data = task });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        if (id != dto.Id) return BadRequest(new { isSuccess = false, message = "ID mismatch." });

        if (!ModelState.IsValid)
        {
            return BadRequest(new { isSuccess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var task = await _taskService.UpdateTaskAsync(dto, User.Identity?.Name, ip);
        if (task == null) return NotFound(new { isSuccess = false, message = "Task not found." });

        return Ok(new { isSuccess = true, data = task });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] TaskItemStatus status)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _taskService.UpdateStatusAsync(id, status, User.Identity?.Name, ip);
        if (!success) return NotFound(new { isSuccess = false, message = "Task not found." });

        return Ok(new { isSuccess = true, message = $"Task status updated to {status}." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.ManagementAccess)]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _taskService.DeleteTaskAsync(id, User.Identity?.Name, ip);
        if (!success) return NotFound(new { isSuccess = false, message = "Task not found or could not be deleted." });

        return Ok(new { isSuccess = true, message = "Task deleted successfully." });
    }
}
