using System.ComponentModel.DataAnnotations;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Application.DTOs.Project;

public class ProjectDto
{
    public int Id { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public PriorityLevel Priority { get; set; }
    public string PriorityName => Priority.ToString();
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int ProgressPercentage => TotalTasks > 0 ? (int)Math.Round((double)CompletedTasks / TotalTasks * 100) : 0;
}

public class CreateProjectDto
{
    [Required]
    [StringLength(20)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal Budget { get; set; }

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(3);

    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public int? ManagerId { get; set; }
}

public class UpdateProjectDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal Budget { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public ProjectStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public int? ManagerId { get; set; }
}
