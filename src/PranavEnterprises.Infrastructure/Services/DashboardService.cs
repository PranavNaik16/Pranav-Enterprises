using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.Dashboard;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Domain.Enums;
using PranavEnterprises.Infrastructure.Data;

namespace PranavEnterprises.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalEmployees = await _context.Employees.CountAsync();
        var totalProjects = await _context.Projects.CountAsync();
        var activeProjects = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.InProgress || p.Status == ProjectStatus.Planning);
        var projectBudgets = await _context.Projects.Select(p => p.Budget).ToListAsync();
        var totalBudget = projectBudgets.Sum();
        var departmentCount = await _context.Departments.CountAsync();

        var totalTasks = await _context.ProjectTasks.CountAsync();
        var completedTasks = await _context.ProjectTasks.CountAsync(t => t.Status == TaskItemStatus.Completed);
        var pendingTasks = totalTasks - completedTasks;

        // Project Status Distribution
        var projectStatusGroups = await _context.Projects
            .GroupBy(p => p.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var projectColors = new Dictionary<ProjectStatus, string>
        {
            { ProjectStatus.Planning, "#38bdf8" },   // sky
            { ProjectStatus.InProgress, "#3b82f6" }, // blue
            { ProjectStatus.OnHold, "#f59e0b" },     // amber
            { ProjectStatus.Completed, "#10b981" },  // emerald
            { ProjectStatus.Cancelled, "#ef4444" }   // red
        };

        var projectDist = projectStatusGroups.Select(g => new ChartDatasetDto
        {
            Label = g.Status.ToString(),
            Value = g.Count,
            Color = projectColors.TryGetValue(g.Status, out var color) ? color : "#6b7280"
        }).ToList();

        // Task Status Distribution
        var taskStatusGroups = await _context.ProjectTasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var taskColors = new Dictionary<TaskItemStatus, string>
        {
            { TaskItemStatus.Todo, "#94a3b8" },        // slate
            { TaskItemStatus.InProgress, "#0284c7" },  // light blue
            { TaskItemStatus.InReview, "#8b5cf6" },    // violet
            { TaskItemStatus.Completed, "#10b981" }    // green
        };

        var taskDist = taskStatusGroups.Select(g => new ChartDatasetDto
        {
            Label = g.Status.ToString(),
            Value = g.Count,
            Color = taskColors.TryGetValue(g.Status, out var color) ? color : "#64748b"
        }).ToList();

        // Department Headcount
        var deptGroups = await _context.Employees
            .Include(e => e.Department)
            .GroupBy(e => e.Department != null ? e.Department.Name : "Unassigned")
            .Select(g => new { Dept = g.Key, Count = g.Count() })
            .ToListAsync();

        var deptColors = new[] { "#2563eb", "#7c3aed", "#059669", "#d97706", "#dc2626", "#0891b2" };
        var deptDist = deptGroups.Select((g, idx) => new ChartDatasetDto
        {
            Label = g.Dept,
            Value = g.Count,
            Color = deptColors[idx % deptColors.Length]
        }).ToList();

        // Monthly trends (sample trend data or aggregated)
        var monthlyTrends = new List<MonthlyTrendDto>
        {
            new() { Month = "Jun", CreatedCount = 8, CompletedCount = 6 },
            new() { Month = "Jul", CreatedCount = 12, CompletedCount = 9 },
            new() { Month = "Aug", CreatedCount = 15, CompletedCount = 13 },
            new() { Month = "Sep", CreatedCount = 11, CompletedCount = 12 },
            new() { Month = "Oct", CreatedCount = 18, CompletedCount = 14 },
            new() { Month = "Nov", CreatedCount = 14, CompletedCount = completedTasks }
        };

        // Recent Activities
        var rawActivities = await _context.ActivityLogs
            .OrderByDescending(a => a.Timestamp)
            .Take(8)
            .ToListAsync();

        var recentActivities = rawActivities.Select(a => new RecentActivityDto
        {
            Id = a.Id,
            UserName = a.UserName,
            UserRole = a.UserRole,
            Action = a.Action,
            Module = a.Module,
            Details = a.Details,
            Timestamp = a.Timestamp,
            TimeAgo = FormatTimeAgo(a.Timestamp)
        }).ToList();

        // Upcoming Deadlines
        var deadlines = await _context.ProjectTasks
            .Include(t => t.Project)
            .Where(t => t.Status != TaskItemStatus.Completed)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .Select(t => new QuickTaskDto
            {
                Id = t.Id,
                TaskCode = t.TaskCode,
                Title = t.Title,
                ProjectName = t.Project != null ? t.Project.Name : "General",
                Priority = t.Priority.ToString(),
                Status = t.Status.ToString(),
                DueDate = t.DueDate
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalUsers = totalUsers,
            TotalEmployees = totalEmployees,
            TotalProjects = totalProjects,
            ActiveProjects = activeProjects,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            TotalTasks = totalTasks,
            TotalBudget = totalBudget,
            DepartmentCount = departmentCount,
            ProjectStatusDistribution = projectDist,
            TaskStatusDistribution = taskDist,
            DepartmentHeadcount = deptDist,
            MonthlyTaskTrends = monthlyTrends,
            RecentActivities = recentActivities,
            UpcomingDeadlines = deadlines
        };
    }

    private static string FormatTimeAgo(DateTime timestamp)
    {
        var diff = DateTime.UtcNow - timestamp;
        if (diff.TotalSeconds < 60) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
        if (diff.TotalDays < 30) return $"{(int)diff.TotalDays}d ago";
        return timestamp.ToString("MMM dd, yyyy");
    }
}
