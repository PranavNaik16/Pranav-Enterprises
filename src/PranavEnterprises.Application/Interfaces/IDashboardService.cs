using PranavEnterprises.Application.DTOs.Dashboard;

namespace PranavEnterprises.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
}
