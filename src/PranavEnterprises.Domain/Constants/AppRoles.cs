namespace PranavEnterprises.Domain.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "Super Admin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Employee = "Employee";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SuperAdmin,
        Admin,
        Manager,
        Employee
    };
}

public static class AppPolicies
{
    public const string RequireSuperAdmin = "RequireSuperAdmin";
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireManager = "RequireManager";
    public const string RequireEmployee = "RequireEmployee";

    // Management policy: Super Admin or Admin
    public const string ManagementAccess = "ManagementAccess";

    // Operational policy: Super Admin, Admin, or Manager
    public const string OperationalAccess = "OperationalAccess";
}
