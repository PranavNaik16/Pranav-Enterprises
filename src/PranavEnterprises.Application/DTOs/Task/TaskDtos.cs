using System.ComponentModel.DataAnnotations;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Application.DTOs.Task;

public class ProjectTaskDto
{
    public int Id { get; set; }
    public string TaskCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int? AssignedEmployeeId { get; set; }
    public string? AssignedEmployeeName { get; set; }
    public DateTime DueDate { get; set; }
    public PriorityLevel Priority { get; set; }
    public string PriorityName => Priority.ToString();
    public TaskItemStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public decimal EstimatedHours { get; set; }
    public decimal LoggedHours { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskDto
{
    [Required]
    [StringLength(20)]
    public string TaskCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int ProjectId { get; set; }

    public int? AssignedEmployeeId { get; set; }

    [Required]
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);

    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;

    [Range(0, 1000)]
    public decimal EstimatedHours { get; set; }
}

public class UpdateTaskDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string TaskCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int ProjectId { get; set; }

    public int? AssignedEmployeeId { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public PriorityLevel Priority { get; set; }
    public TaskItemStatus Status { get; set; }

    [Range(0, 1000)]
    public decimal EstimatedHours { get; set; }

    [Range(0, 1000)]
    public decimal LoggedHours { get; set; }
}
