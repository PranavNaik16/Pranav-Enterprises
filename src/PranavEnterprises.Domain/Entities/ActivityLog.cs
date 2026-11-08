using PranavEnterprises.Domain.Common;

namespace PranavEnterprises.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public string? UserId { get; set; }
    public string UserName { get; set; } = "System";
    public string UserRole { get; set; } = "System";
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, Logout, StatusChange
    public string Module { get; set; } = string.Empty; // Employee, Project, Task, User, Role, Auth
    public string? EntityId { get; set; }
    public string Details { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
