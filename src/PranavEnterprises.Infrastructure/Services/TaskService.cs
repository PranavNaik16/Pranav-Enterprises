using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.Task;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;
using PranavEnterprises.Infrastructure.Data;

namespace PranavEnterprises.Infrastructure.Services;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public TaskService(ApplicationDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<List<ProjectTaskDto>> GetAllTasksAsync(int? projectId = null, int? employeeId = null, TaskItemStatus? status = null)
    {
        var query = _context.ProjectTasks
            .Include(t => t.Project)
            .Include(t => t.AssignedEmployee)
            .AsNoTracking();

        if (projectId.HasValue && projectId.Value > 0)
        {
            query = query.Where(t => t.ProjectId == projectId.Value);
        }

        if (employeeId.HasValue && employeeId.Value > 0)
        {
            query = query.Where(t => t.AssignedEmployeeId == employeeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new ProjectTaskDto
            {
                Id = t.Id,
                TaskCode = t.TaskCode,
                Title = t.Title,
                Description = t.Description,
                ProjectId = t.ProjectId,
                ProjectName = t.Project != null ? t.Project.Name : "Unassigned",
                AssignedEmployeeId = t.AssignedEmployeeId,
                AssignedEmployeeName = t.AssignedEmployee != null ? t.AssignedEmployee.FullName : "Unassigned",
                DueDate = t.DueDate,
                Priority = t.Priority,
                Status = t.Status,
                EstimatedHours = t.EstimatedHours,
                LoggedHours = t.LoggedHours,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProjectTaskDto?> GetTaskByIdAsync(int id)
    {
        var t = await _context.ProjectTasks
            .Include(x => x.Project)
            .Include(x => x.AssignedEmployee)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (t == null) return null;

        return new ProjectTaskDto
        {
            Id = t.Id,
            TaskCode = t.TaskCode,
            Title = t.Title,
            Description = t.Description,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name ?? "Unassigned",
            AssignedEmployeeId = t.AssignedEmployeeId,
            AssignedEmployeeName = t.AssignedEmployee?.FullName ?? "Unassigned",
            DueDate = t.DueDate,
            Priority = t.Priority,
            Status = t.Status,
            EstimatedHours = t.EstimatedHours,
            LoggedHours = t.LoggedHours,
            CreatedAt = t.CreatedAt
        };
    }

    public async Task<ProjectTaskDto> CreateTaskAsync(CreateTaskDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var task = new ProjectTask
        {
            TaskCode = dto.TaskCode,
            Title = dto.Title,
            Description = dto.Description,
            ProjectId = dto.ProjectId,
            AssignedEmployeeId = dto.AssignedEmployeeId,
            DueDate = dto.DueDate,
            Priority = dto.Priority,
            Status = dto.Status,
            EstimatedHours = dto.EstimatedHours,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectTasks.Add(task);
        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Create",
            "Task",
            task.Id.ToString(),
            $"Created task '{task.Title}' ({task.TaskCode}) under project ID {task.ProjectId}.",
            ipAddress);

        return (await GetTaskByIdAsync(task.Id))!;
    }

    public async Task<ProjectTaskDto?> UpdateTaskAsync(UpdateTaskDto dto, string? currentUserName = null, string? ipAddress = null)
    {
        var task = await _context.ProjectTasks.FindAsync(dto.Id);
        if (task == null) return null;

        task.TaskCode = dto.TaskCode;
        task.Title = dto.Title;
        task.Description = dto.Description;
        task.ProjectId = dto.ProjectId;
        task.AssignedEmployeeId = dto.AssignedEmployeeId;
        task.DueDate = dto.DueDate;
        task.Priority = dto.Priority;
        task.Status = dto.Status;
        task.EstimatedHours = dto.EstimatedHours;
        task.LoggedHours = dto.LoggedHours;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Update",
            "Task",
            task.Id.ToString(),
            $"Updated task '{task.Title}'.",
            ipAddress);

        return (await GetTaskByIdAsync(task.Id))!;
    }

    public async Task<bool> DeleteTaskAsync(int id, string? currentUserName = null, string? ipAddress = null)
    {
        var task = await _context.ProjectTasks.FindAsync(id);
        if (task == null) return false;

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "Delete",
            "Task",
            id.ToString(),
            $"Soft-deleted task '{task.Title}'.",
            ipAddress);

        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, TaskItemStatus status, string? currentUserName = null, string? ipAddress = null)
    {
        var task = await _context.ProjectTasks.FindAsync(id);
        if (task == null) return false;

        var oldStatus = task.Status;
        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogAsync(
            null,
            currentUserName ?? "System",
            "User",
            "StatusChange",
            "Task",
            id.ToString(),
            $"Changed task '{task.Title}' status from {oldStatus} to {status}.",
            ipAddress);

        return true;
    }
}
