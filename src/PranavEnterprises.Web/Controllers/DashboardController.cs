using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Application.Interfaces;

namespace PranavEnterprises.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetDashboardSummaryAsync();
        return View(summary);
    }
}
