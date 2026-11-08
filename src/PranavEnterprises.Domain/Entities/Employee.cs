using PranavEnterprises.Domain.Common;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Domain.Entities;

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? AvatarUrl { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}
