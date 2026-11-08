using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PranavEnterprises.Domain.Constants;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;

namespace PranavEnterprises.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger)
    {
        try
        {
            // Apply migrations or ensure database is created
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            var roles = new List<(string Name, string Description)>
            {
                (AppRoles.SuperAdmin, "Full system administrative control with security & role management"),
                (AppRoles.Admin, "Administrative operations, user management, and operational analytics"),
                (AppRoles.Manager, "Project planning, task delegation, team tracking, and resource oversight"),
                (AppRoles.Employee, "Task execution, progress updates, and personal profile management")
            };

            foreach (var (name, desc) in roles)
            {
                if (!await roleManager.RoleExistsAsync(name))
                {
                    await roleManager.CreateAsync(new ApplicationRole(name, desc));
                    logger.LogInformation("Role seeded: {Role}", name);
                }
            }

            // 2. Seed Default Users
            var defaultUsers = new List<(string Email, string First, string Last, string Role, string Title, string Dept)>
            {
                ("superadmin@pranav.com", "Pranav", "Naik", AppRoles.SuperAdmin, "Chief Technology Officer", "Executive"),
                ("admin@pranav.com", "Amruta", "Naik", AppRoles.Admin, "Operations Director", "Operations"),
                ("manager@pranav.com", "Shubham", "Naik", AppRoles.Manager, "Senior Project Manager", "Engineering"),
                ("employee@pranav.com", "Ram", "Naik", AppRoles.Employee, "Full Stack Developer", "Engineering")
            };

            foreach (var u in defaultUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(u.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = u.Email,
                        Email = u.Email,
                        FirstName = u.First,
                        LastName = u.Last,
                        JobTitle = u.Title,
                        DepartmentName = u.Dept,
                        EmailConfirmed = true,
                        IsActive = true,
                        IsApproved = true,
                        ApprovedAt = DateTime.UtcNow,
                        ApprovedBy = "System Provisioning",
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(user, "P@ssword123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, u.Role);
                        logger.LogInformation("User seeded: {Email} with role {Role}", u.Email, u.Role);
                    }
                    else
                    {
                        logger.LogError("Failed to seed user {Email}: {Errors}", u.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    // Ensure names match latest updates and status is approved
                    existingUser.FirstName = u.First;
                    existingUser.LastName = u.Last;
                    existingUser.IsApproved = true;
                    existingUser.IsActive = true;
                    await userManager.UpdateAsync(existingUser);
                }
            }

            // 3. Seed Departments
            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new() { Name = "Software Engineering", Code = "ENG", Description = "Core product engineering, architecture, and cloud platforms" },
                    new() { Name = "Product Management", Code = "PM", Description = "Product lifecycle, UX research, and customer requirements" },
                    new() { Name = "Quality Assurance & DevOps", Code = "QA", Description = "CI/CD pipelines, automated testing, and infrastructure reliability" },
                    new() { Name = "Human Resources", Code = "HR", Description = "Talent acquisition, employee engagement, and organizational development" },
                    new() { Name = "Finance & Accounting", Code = "FIN", Description = "Financial budgeting, auditing, payroll, and fiscal reporting" },
                    new() { Name = "Sales & Client Success", Code = "SLS", Description = "Enterprise accounts, client relations, and commercial growth" }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded 6 departments.");
            }

            var engDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "ENG") ?? await context.Departments.FirstAsync();
            var pmDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "PM") ?? engDept;
            var qaDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "QA") ?? engDept;
            var hrDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "HR") ?? engDept;
            var finDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "FIN") ?? engDept;
            var slsDept = await context.Departments.FirstOrDefaultAsync(d => d.Code == "SLS") ?? engDept;

            // 4. Seed Employees
            if (!await context.Employees.AnyAsync())
            {
                var employees = new List<Employee>
                {
                    new()
                    {
                        EmployeeCode = "EMP-1001",
                        FirstName = "Shubham",
                        LastName = "Naik",
                        Email = "manager@pranav.com",
                        Phone = "+91 98765 43210",
                        Position = "Senior Technical Project Manager",
                        Salary = 125000,
                        HireDate = DateTime.UtcNow.AddYears(-3),
                        Status = EmployeeStatus.Active,
                        DepartmentId = engDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1002",
                        FirstName = "Ram",
                        LastName = "Naik",
                        Email = "employee@pranav.com",
                        Phone = "+91 98765 43211",
                        Position = "Lead Full Stack .NET Engineer",
                        Salary = 95000,
                        HireDate = DateTime.UtcNow.AddYears(-2),
                        Status = EmployeeStatus.Active,
                        DepartmentId = engDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1003",
                        FirstName = "Rohan",
                        LastName = "Verma",
                        Email = "rohan.verma@pranav.com",
                        Phone = "+91 98765 43212",
                        Position = "Principal Cloud Architect",
                        Salary = 140000,
                        HireDate = DateTime.UtcNow.AddYears(-4),
                        Status = EmployeeStatus.Active,
                        DepartmentId = engDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1004",
                        FirstName = "Pooja",
                        LastName = "Deshmukh",
                        Email = "pooja.d@pranav.com",
                        Phone = "+91 98765 43213",
                        Position = "Product Strategy Director",
                        Salary = 115000,
                        HireDate = DateTime.UtcNow.AddYears(-2).AddMonths(-4),
                        Status = EmployeeStatus.Active,
                        DepartmentId = pmDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1005",
                        FirstName = "Karthik",
                        LastName = "Iyer",
                        Email = "karthik.iyer@pranav.com",
                        Phone = "+91 98765 43214",
                        Position = "DevOps & SRE Lead",
                        Salary = 105000,
                        HireDate = DateTime.UtcNow.AddYears(-1).AddMonths(-8),
                        Status = EmployeeStatus.Active,
                        DepartmentId = qaDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1006",
                        FirstName = "Sneha",
                        LastName = "Kapoor",
                        Email = "sneha.k@pranav.com",
                        Phone = "+91 98765 43215",
                        Position = "Senior QA Automation Specialist",
                        Salary = 82000,
                        HireDate = DateTime.UtcNow.AddYears(-1),
                        Status = EmployeeStatus.Active,
                        DepartmentId = qaDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1007",
                        FirstName = "Rajesh",
                        LastName = "Nair",
                        Email = "rajesh.n@pranav.com",
                        Phone = "+91 98765 43216",
                        Position = "Head of Human Resources",
                        Salary = 98000,
                        HireDate = DateTime.UtcNow.AddYears(-3).AddMonths(-2),
                        Status = EmployeeStatus.Active,
                        DepartmentId = hrDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1008",
                        FirstName = "Meera",
                        LastName = "Sengupta",
                        Email = "meera.s@pranav.com",
                        Phone = "+91 98765 43217",
                        Position = "Financial Controller",
                        Salary = 110000,
                        HireDate = DateTime.UtcNow.AddYears(-2).AddMonths(-6),
                        Status = EmployeeStatus.Active,
                        DepartmentId = finDept.Id
                    },
                    new()
                    {
                        EmployeeCode = "EMP-1009",
                        FirstName = "Arjun",
                        LastName = "Reddy",
                        Email = "arjun.r@pranav.com",
                        Phone = "+91 98765 43218",
                        Position = "Enterprise Sales Executive",
                        Salary = 92000,
                        HireDate = DateTime.UtcNow.AddYears(-1).AddMonths(-3),
                        Status = EmployeeStatus.Active,
                        DepartmentId = slsDept.Id
                    }
                };

                await context.Employees.AddRangeAsync(employees);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded 9 employees.");
            }

            var mgrEmp = await context.Employees.FirstOrDefaultAsync(e => e.Email == "manager@pranav.com") ?? await context.Employees.FirstAsync();
            if (mgrEmp != null && (mgrEmp.FirstName != "Shubham" || mgrEmp.LastName != "Naik"))
            {
                mgrEmp.FirstName = "Shubham";
                mgrEmp.LastName = "Naik";
            }

            var devEmp = await context.Employees.FirstOrDefaultAsync(e => e.Email == "employee@pranav.com") ?? mgrEmp;
            if (devEmp != null && (devEmp.FirstName != "Ram" || devEmp.LastName != "Naik"))
            {
                devEmp.FirstName = "Ram";
                devEmp.LastName = "Naik";
            }
            await context.SaveChangesAsync();
            var rohanEmp = await context.Employees.FirstOrDefaultAsync(e => e.FirstName == "Rohan") ?? devEmp;
            var poojaEmp = await context.Employees.FirstOrDefaultAsync(e => e.FirstName == "Pooja") ?? devEmp;

            // 5. Seed Projects
            if (!await context.Projects.AnyAsync())
            {
                var projects = new List<Project>
                {
                    new()
                    {
                        ProjectCode = "PRJ-2026-01",
                        Name = "Pranav Enterprise Cloud ERP Modernization",
                        Description = "Next-generation ERP web portal architecture with multi-tenant database support, real-time analytics, and microservices integration.",
                        ClientName = "Global Logistics Corp",
                        Budget = 450000,
                        StartDate = DateTime.UtcNow.AddMonths(-3),
                        EndDate = DateTime.UtcNow.AddMonths(5),
                        Status = ProjectStatus.InProgress,
                        Priority = PriorityLevel.Critical,
                        ManagerId = mgrEmp.Id
                    },
                    new()
                    {
                        ProjectCode = "PRJ-2026-02",
                        Name = "Enterprise AI Insights & Analytics Hub",
                        Description = "Predictive intelligence dashboard utilizing machine learning pipelines for sales forecasting and customer churn risk analysis.",
                        ClientName = "Apex Financial Solutions",
                        Budget = 280000,
                        StartDate = DateTime.UtcNow.AddMonths(-2),
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        Status = ProjectStatus.InProgress,
                        Priority = PriorityLevel.High,
                        ManagerId = mgrEmp.Id
                    },
                    new()
                    {
                        ProjectCode = "PRJ-2026-03",
                        Name = "Unified Customer & Partner Portal",
                        Description = "Self-service web platform with dynamic role-based access control, billing integration, and document management.",
                        ClientName = "Starlight Retailers Inc",
                        Budget = 185000,
                        StartDate = DateTime.UtcNow.AddMonths(-5),
                        EndDate = DateTime.UtcNow.AddMonths(-1),
                        Status = ProjectStatus.Completed,
                        Priority = PriorityLevel.Medium,
                        ManagerId = mgrEmp.Id
                    },
                    new()
                    {
                        ProjectCode = "PRJ-2026-04",
                        Name = "Automated Supply Chain Orchestrator",
                        Description = "IoT-connected telemetry engine for warehouse tracking, inventory optimization, and automated dispatch management.",
                        ClientName = "TransContinental Freight",
                        Budget = 320000,
                        StartDate = DateTime.UtcNow.AddMonths(-1),
                        EndDate = DateTime.UtcNow.AddMonths(6),
                        Status = ProjectStatus.InProgress,
                        Priority = PriorityLevel.High,
                        ManagerId = mgrEmp.Id
                    },
                    new()
                    {
                        ProjectCode = "PRJ-2026-05",
                        Name = "Zero-Trust Infrastructure & Identity Hardening",
                        Description = "Implementation of enterprise-wide zero trust architecture, OAuth2/OIDC, and automated vulnerability scanning.",
                        ClientName = "Internal Governance",
                        Budget = 120000,
                        StartDate = DateTime.UtcNow.AddDays(-15),
                        EndDate = DateTime.UtcNow.AddMonths(2),
                        Status = ProjectStatus.Planning,
                        Priority = PriorityLevel.Critical,
                        ManagerId = mgrEmp.Id
                    }
                };

                await context.Projects.AddRangeAsync(projects);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded 5 projects.");
            }

            var prj1 = await context.Projects.FirstOrDefaultAsync(p => p.ProjectCode == "PRJ-2026-01") ?? await context.Projects.FirstAsync();
            var prj2 = await context.Projects.FirstOrDefaultAsync(p => p.ProjectCode == "PRJ-2026-02") ?? prj1;
            var prj3 = await context.Projects.FirstOrDefaultAsync(p => p.ProjectCode == "PRJ-2026-03") ?? prj1;
            var prj4 = await context.Projects.FirstOrDefaultAsync(p => p.ProjectCode == "PRJ-2026-04") ?? prj1;

            // 6. Seed Project Tasks
            if (!await context.ProjectTasks.AnyAsync())
            {
                var tasks = new List<ProjectTask>
                {
                    new()
                    {
                        TaskCode = "TSK-101",
                        Title = "Implement Identity Authentication & JWT Bearer Generation",
                        Description = "Configure ASP.NET Core Identity with password hashing, cookie authentication, and JWT token issuance endpoints.",
                        ProjectId = prj1.Id,
                        AssignedEmployeeId = devEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(3),
                        Priority = PriorityLevel.Critical,
                        Status = TaskItemStatus.Completed,
                        EstimatedHours = 24,
                        LoggedHours = 22
                    },
                    new()
                    {
                        TaskCode = "TSK-102",
                        Title = "Design Interactive Administrative Dashboard UI",
                        Description = "Create responsive Bootstrap 5 dashboard layout with Chart.js KPI cards, task breakdown charts, and navigation bar.",
                        ProjectId = prj1.Id,
                        AssignedEmployeeId = devEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(5),
                        Priority = PriorityLevel.High,
                        Status = TaskItemStatus.Completed,
                        EstimatedHours = 32,
                        LoggedHours = 30
                    },
                    new()
                    {
                        TaskCode = "TSK-103",
                        Title = "Build Employee & Department Management CRUD Views",
                        Description = "Develop full CRUD operations for Employees module with search filters, modal forms, and validation rules.",
                        ProjectId = prj1.Id,
                        AssignedEmployeeId = devEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(7),
                        Priority = PriorityLevel.High,
                        Status = TaskItemStatus.InProgress,
                        EstimatedHours = 28,
                        LoggedHours = 14
                    },
                    new()
                    {
                        TaskCode = "TSK-104",
                        Title = "Setup Role-Based Authorization Policies & Filters",
                        Description = "Enforce Super Admin, Admin, Manager, and Employee access levels across MVC controllers and REST endpoints.",
                        ProjectId = prj1.Id,
                        AssignedEmployeeId = rohanEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(10),
                        Priority = PriorityLevel.Critical,
                        Status = TaskItemStatus.InProgress,
                        EstimatedHours = 20,
                        LoggedHours = 12
                    },
                    new()
                    {
                        TaskCode = "TSK-105",
                        Title = "Construct AI Analytics KPI Data Aggregation Engine",
                        Description = "Write background query services to compute weekly sales velocity and customer churn metric datasets.",
                        ProjectId = prj2.Id,
                        AssignedEmployeeId = devEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(14),
                        Priority = PriorityLevel.Medium,
                        Status = TaskItemStatus.Todo,
                        EstimatedHours = 40,
                        LoggedHours = 0
                    },
                    new()
                    {
                        TaskCode = "TSK-106",
                        Title = "Client Portal Payment Gateway Webhook Handler",
                        Description = "Validate incoming stripe/paypal signature headers and update project subscription records.",
                        ProjectId = prj3.Id,
                        AssignedEmployeeId = devEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(-10),
                        Priority = PriorityLevel.High,
                        Status = TaskItemStatus.Completed,
                        EstimatedHours = 18,
                        LoggedHours = 18
                    },
                    new()
                    {
                        TaskCode = "TSK-107",
                        Title = "Warehouse Telemetry Stream Event Listener",
                        Description = "Process incoming MQTT telemetry payload packets and trigger stock reorder thresholds.",
                        ProjectId = prj4.Id,
                        AssignedEmployeeId = rohanEmp.Id,
                        DueDate = DateTime.UtcNow.AddDays(18),
                        Priority = PriorityLevel.Medium,
                        Status = TaskItemStatus.Todo,
                        EstimatedHours = 36,
                        LoggedHours = 4
                    }
                };

                await context.ProjectTasks.AddRangeAsync(tasks);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded 7 project tasks.");
            }

            // 7. Seed Initial Activity Logs
            if (!await context.ActivityLogs.AnyAsync())
            {
                var logs = new List<ActivityLog>
                {
                    new()
                    {
                        UserName = "superadmin@pranav.com",
                        UserRole = AppRoles.SuperAdmin,
                        Action = "SystemSetup",
                        Module = "System",
                        Details = "Initialized enterprise system database with clean architecture seed configuration.",
                        Timestamp = DateTime.UtcNow.AddDays(-3)
                    },
                    new()
                    {
                        UserName = "admin@pranav.com",
                        UserRole = AppRoles.Admin,
                        Action = "Create",
                        Module = "Department",
                        Details = "Configured 6 primary enterprise operational departments.",
                        Timestamp = DateTime.UtcNow.AddDays(-2)
                    },
                    new()
                    {
                        UserName = "manager@pranav.com",
                        UserRole = AppRoles.Manager,
                        Action = "Create",
                        Module = "Project",
                        EntityId = prj1.Id.ToString(),
                        Details = $"Created strategic project '{prj1.Name}' with budget of {prj1.Budget:C}.",
                        Timestamp = DateTime.UtcNow.AddDays(-1)
                    },
                    new()
                    {
                        UserName = "employee@pranav.com",
                        UserRole = AppRoles.Employee,
                        Action = "Update",
                        Module = "Task",
                        Details = "Completed task 'TSK-101: Implement Identity Authentication & JWT Bearer Generation'.",
                        Timestamp = DateTime.UtcNow.AddHours(-4)
                    }
                };

                await context.ActivityLogs.AddRangeAsync(logs);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded initial activity logs.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the enterprise database.");
            throw;
        }
    }
}
