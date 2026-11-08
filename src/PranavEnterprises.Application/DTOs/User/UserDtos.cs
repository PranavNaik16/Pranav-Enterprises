using System.ComponentModel.DataAnnotations;

namespace PranavEnterprises.Application.DTOs.User;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public string? RequestedRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class ApproveUserDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string ApprovedRole { get; set; } = "Employee";
}

public class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Employee";

    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
}

public class UpdateUserDto
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Employee";

    public string? JobTitle { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UsersCount { get; set; }
}

public class CreateRoleDto
{
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Description { get; set; }
}
