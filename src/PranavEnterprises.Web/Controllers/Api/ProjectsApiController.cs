using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Application.DTOs.Project;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Web.Controllers.Api;

[ApiController]
[Route("api/projects")]
[Produces("application/json")]
[Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},Identity.Application")]
public class ProjectsApiController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsApiController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] ProjectStatus? status)
    {
        var projects = await _projectService.GetAllProjectsAsync(search, status);
        return Ok(new { isSuccess = true, count = projects.Count, data = projects });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound(new { isSuccess = false, message = "Project not found." });

        return Ok(new { isSuccess = true, data = project });
    }

    [HttpPost]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { isSuccess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var project = await _projectService.CreateProjectAsync(dto, User.Identity?.Name, ip);

        return CreatedAtAction(nameof(GetById), new { id = project.Id }, new { isSuccess = true, data = project });
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDto dto)
    {
        if (id != dto.Id) return BadRequest(new { isSuccess = false, message = "ID mismatch." });

        if (!ModelState.IsValid)
        {
            return BadRequest(new { isSuccess = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var project = await _projectService.UpdateProjectAsync(dto, User.Identity?.Name, ip);
        if (project == null) return NotFound(new { isSuccess = false, message = "Project not found." });

        return Ok(new { isSuccess = true, data = project });
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = AppPolicies.OperationalAccess)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ProjectStatus status)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _projectService.UpdateStatusAsync(id, status, User.Identity?.Name, ip);
        if (!success) return NotFound(new { isSuccess = false, message = "Project not found." });

        return Ok(new { isSuccess = true, message = $"Status updated to {status}." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.ManagementAccess)]
    public async Task<IActionResult> Delete(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var success = await _projectService.DeleteProjectAsync(id, User.Identity?.Name, ip);
        if (!success) return NotFound(new { isSuccess = false, message = "Project not found or could not be deleted." });

        return Ok(new { isSuccess = true, message = "Project deleted successfully." });
    }
}
