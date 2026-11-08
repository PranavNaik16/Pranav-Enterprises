using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Constants;

namespace PranavEnterprises.Web.Controllers;

[Authorize(Policy = AppPolicies.ManagementAccess)]
public class ActivityLogsController : Controller
{
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var logs = await _activityLogService.GetRecentLogsAsync(100);
        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv()
    {
        var logs = await _activityLogService.GetRecentLogsAsync(500);
        var sb = new StringBuilder();
        sb.AppendLine("TimestampUTC,UserName,UserRole,Action,Module,EntityId,Details,IpAddress");
        foreach (var l in logs)
        {
            sb.AppendLine($"\"{l.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{l.UserName}\",\"{l.UserRole}\",\"{l.Action}\",\"{l.Module}\",\"{l.EntityId}\",\"{l.Details.Replace("\"", "\"\"")}\",\"{l.IpAddress}\"");
        }
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"ActivityLogs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
