using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.Project;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;
using PranavEnterprises.Infrastructure.Data;

namespace PranavEnterprises.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public ProjectService(ApplicationDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<List<ProjectDto>> GetAllProjectsAsync(string? search = null, ProjectStatus? status = null)
    {
        var query = _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Tasks)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(s) ||
                p.ProjectCode.ToLower().Contains(s) ||
                p.ClientName.ToLower().Contains(s));
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectDto
            {
                Id = p.Id,
                ProjectCode = p.ProjectCode,
                Name = p.Name,
                Description = p.Description,
                ClientName = p.ClientName,
                Budget = p.Budget,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
                Priority = p.Priority,
                ManagerId = p.ManagerId,
                ManagerName = p.Manager != null ? p.Manager.FullName : "Unassigned",
                TotalTasks = p.Tasks.Count,
                CompletedTasks = p.Tasks.Count(t => t.Status == TaskItemStatus.Completed)
            })
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var p = await _context.Projects
            .Include(x => x.Manager)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null) return null;

        return new ProjectDto
        {
            Id = p.Id,
            ProjectCode = p.ProjectCode,
            Name = p.Name,
            Description = p.Description,
            ClientName = p.ClientName,
            Budget = p.Budget,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Status = p.Status,
            Priority = p.Priority,
            ManagerId = p.ManagerId,
            ManagerName = p.Manager?.FullName ?? "Unassigned",
            TotalTasks = p.Tasks.Count,
            CompletedTasks = p.Tasks.Count(t => t.Status == TaskItemStatus.Completed)
        };
    }

    public async Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var project = new Project
        {
            ProjectCode = dto.ProjectCode,
            Name = dto.Name,
            Description = dto.Description,
            ClientName = dto.ClientName,
            Budget = dto.Budget,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            Priority = dto.Priority,
            ManagerId = dto.ManagerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Create",
            "Project",
            project.Id.ToString(),
            $"Created strategic project '{project.Name}' ({project.ProjectCode}).",
            ipAddress);

        return (await GetProjectByIdAsync(project.Id))!;
    }

    public async Task<ProjectDto?> UpdateProjectAsync(UpdateProjectDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var project = await _context.Projects.FindAsync(dto.Id);
        if (project == null) return null;

        project.ProjectCode = dto.ProjectCode;
        project.Name = dto.Name;
        project.Description = dto.Description;
        project.ClientName = dto.ClientName;
        project.Budget = dto.Budget;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        project.Status = dto.Status;
        project.Priority = dto.Priority;
        project.ManagerId = dto.ManagerId;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Update",
            "Project",
            project.Id.ToString(),
            $"Updated project '{project.Name}' details.",
            ipAddress);

        return (await GetProjectByIdAsync(project.Id))!;
    }

    public async Task<bool> DeleteProjectAsync(int id, string? currentUserName = null, string? ipAddress = null)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        project.IsDeleted = true;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Delete",
            "Project",
            id.ToString(),
            $"Soft-deleted project '{project.Name}'.",
            ipAddress);

        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, ProjectStatus status, string? currentUserName = null, string? ipAddress = null)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        var oldStatus = project.Status;
        project.Status = status;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "StatusChange",
            "Project",
            id.ToString(),
            $"Changed status of '{project.Name}' from {oldStatus} to {status}.",
            ipAddress);

        return true;
    }
}
