using Microsoft.EntityFrameworkCore;
using PranavEnterprises.Application.DTOs.Activity;
using PranavEnterprises.Application.Interfaces;
using PranavEnterprises.Domain.Entities;
using PranavEnterprises.Infrastructure.Data;

namespace PranavEnterprises.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly ApplicationDbContext _context;

    public ActivityLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActivityLogDto>> GetRecentLogsAsync(int count = 50)
    {
        return await _context.ActivityLogs
            .OrderByDescending(x => x.Timestamp)
            .Take(count)
            .Select(x => new ActivityLogDto
            {
                Id = x.Id,
                UserId = x.UserId,
                UserName = x.UserName,
                UserRole = x.UserRole,
                Action = x.Action,
                Module = x.Module,
                EntityId = x.EntityId,
                Details = x.Details,
                IpAddress = x.IpAddress,
                Timestamp = x.Timestamp
            })
            .ToListAsync();
    }

    public async Task LogAsync(
        string? userId,
        string userName,
        string userRole,
        string action,
        string module,
        string? entityId,
        string details,
        string? ipAddress = null)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            UserName = userName,
            UserRole = userRole,
            Action = action,
            Module = module,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        _context.ActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }
}
