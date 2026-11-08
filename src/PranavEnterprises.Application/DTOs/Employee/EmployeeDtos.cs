using System.ComponentModel.DataAnnotations;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Application.DTOs.Employee;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Position { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    public DateTime HireDate { get; set; }
    public EmployeeStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int ActiveTasksCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmployeeDto
{
    [Required(ErrorMessage = "Employee code is required")]
    [StringLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Position is required")]
    public string Position { get; set; } = string.Empty;

    [Range(0, 10000000, ErrorMessage = "Salary must be a positive value")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Hire date is required")]
    public DateTime HireDate { get; set; } = DateTime.UtcNow;

    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;

    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }
}

public class UpdateEmployeeDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Employee code is required")]
    [StringLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Position is required")]
    public string Position { get; set; } = string.Empty;

    [Range(0, 10000000, ErrorMessage = "Salary must be a positive value")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Hire date is required")]
    public DateTime HireDate { get; set; }

    public EmployeeStatus Status { get; set; }

    [Required(ErrorMessage = "Department is required")]
    public int DepartmentId { get; set; }
}
