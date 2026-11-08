using PranavEnterprises.Domain.Common;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Domain.Entities;

public class Project : BaseEntity
{
    public string ProjectCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.AddMonths(3);
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;

    public int? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
