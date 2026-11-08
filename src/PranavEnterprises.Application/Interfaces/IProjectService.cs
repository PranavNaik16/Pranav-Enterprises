using PranavEnterprises.Application.DTOs.Project;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Application.Interfaces;

public interface IProjectService
{
    Task<List<ProjectDto>> GetAllProjectsAsync(string? search = null, ProjectStatus? status = null);
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<ProjectDto> CreateProjectAsync(CreateProjectDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<ProjectDto?> UpdateProjectAsync(UpdateProjectDto dto, string? currentUserName = null, string? ipAddress = null);
    Task<bool> DeleteProjectAsync(int id, string? currentUserName = null, string? ipAddress = null);
    Task<bool> UpdateStatusAsync(int id, ProjectStatus status, string? currentUserName = null, string? ipAddress = null);
}
