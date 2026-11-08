using PranavEnterprises.Application.DTOs.Activity;

namespace PranavEnterprises.Application.Interfaces;

public interface IActivityLogService
{
    Task<List<ActivityLogDto>> GetRecentLogsAsync(int count = 50);
    Task LogAsync(string? userId, string userName, string userRole, string action, string module, string? entityId, string details, string? ipAddress = null);
}
