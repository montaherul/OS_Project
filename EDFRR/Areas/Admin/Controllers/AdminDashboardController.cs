using EDFRR.Models.DTOs;
using EDFRR.Models.ViewModels;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : Controller
{
    private readonly IAdminDashboardService _dashboardService;
    private readonly ILogger<AdminDashboardController> _logger;

    public AdminDashboardController(IAdminDashboardService dashboardService, ILogger<AdminDashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var data = await _dashboardService.GetFullDashboardAsync();
            return View(new AdminDashboardViewModel { Stats = data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            TempData["Error"] = "Error loading dashboard.";
            return View(new AdminDashboardViewModel());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardStats()
    {
        try
        {
            var data = await _dashboardService.GetDashboardStatsAsync();
            return Json(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching admin dashboard stats");
            return Json(new AdminDashboardDto());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentUsers(int count = 10)
    {
        var users = await _dashboardService.GetRecentUsersAsync(count);
        return Json(users);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentSessions(int count = 10)
    {
        var sessions = await _dashboardService.GetRecentSessionsAsync(count);
        return Json(sessions);
    }

    [HttpGet]
    public async Task<IActionResult> GetAlgorithmUsage()
    {
        var usage = await _dashboardService.GetAlgorithmUsageAsync();
        return Json(usage);
    }

    [HttpGet]
    public async Task<IActionResult> GetProcessTrend()
    {
        var trend = await _dashboardService.GetProcessTrendAsync();
        return Json(trend);
    }

    [HttpGet]
    public async Task<IActionResult> GetSessionTrend()
    {
        var trend = await _dashboardService.GetSessionTrendAsync();
        return Json(trend);
    }

    [HttpGet]
    public async Task<IActionResult> GetSimulationTrend()
    {
        var trend = await _dashboardService.GetSimulationTrendAsync();
        return Json(trend);
    }

    [HttpGet]
    public async Task<IActionResult> GetTopUsersByProcessCount(int count = 10)
    {
        var users = await _dashboardService.GetTopUsersByProcessCountAsync(count);
        return Json(users);
    }
}
