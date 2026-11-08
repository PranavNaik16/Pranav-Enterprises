using PranavEnterprises.Application.DTOs.Task;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Application.Interfaces;

public interface ITaskService
{
    Task<List<ProjectTaskDto>> GetAllTasksAsync(int? projectId = null, int? employeeId = null, TaskItemStatus? status = null);
    Task<ProjectTaskDto?> GetTaskByIdAsync(int id);
    Task<ProjectTaskDto> CreateTaskAsync(CreateTaskDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<ProjectTaskDto?> UpdateTaskAsync(UpdateTaskDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<bool> DeleteTaskAsync(int id, string? currentUserName = null, string? ipAddress = null);
    Task<bool> UpdateStatusAsync(int id, TaskItemStatus status, string? currentUserName = null, string? ipAddress = null);
}
