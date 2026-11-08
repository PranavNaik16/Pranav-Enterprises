namespace PranavEnterprises.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalProjects { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int TotalTasks { get; set; }
    public decimal TotalBudget { get; set; }
    public int DepartmentCount { get; set; }

    public List<ChartDatasetDto> ProjectStatusDistribution { get; set; } = new();
    public List<ChartDatasetDto> TaskStatusDistribution { get; set; } = new();
    public List<ChartDatasetDto> DepartmentHeadcount { get; set; } = new();
    public List<MonthlyTrendDto> MonthlyTaskTrends { get; set; } = new();

    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public List<QuickTaskDto> UpcomingDeadlines { get; set; } = new();
}

public class ChartDatasetDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public string Color { get; set; } = "#3b82f6";
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int CreatedCount { get; set; }
    public int CompletedCount { get; set; }
}

public class RecentActivityDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
}

public class QuickTaskDto
{
    public int Id { get; set; }
    public string TaskCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
}
