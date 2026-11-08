using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PranavEnterprises.Web.Models;

namespace PranavEnterprises.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? traceId = null)
    {
        return View(new ErrorViewModel
        {
            RequestId = traceId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
