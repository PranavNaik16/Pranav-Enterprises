using PranavEnterprises.Domain.Common;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Domain.Entities;

public class ProjectTask : BaseEntity
{
    public string TaskCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public Project? Project { get; set; }

    public int? AssignedEmployeeId { get; set; }
    public Employee? AssignedEmployee { get; set; }

    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public decimal EstimatedHours { get; set; }
    public decimal LoggedHours { get; set; }
}
