using Microsoft.AspNetCore.Identity;

namespace PranavEnterprises.Domain.Entities;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName, string? description = null) : base(roleName)
    {
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
