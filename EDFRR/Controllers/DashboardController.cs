using System.Security.Claims;
using EDFRR.Models.DTOs;
using EDFRR.Models.ViewModels;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ISessionService sessionService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? sessionId)
    {
        try
        {
            var isAdmin = User.IsInRole("Admin");
            DashboardDto data;

            if (sessionId.HasValue)
            {
                data = await _dashboardService.GetDashboardDataForSessionAsync(sessionId.Value);
            }
            else if (isAdmin)
            {
                data = await _dashboardService.GetAdminDashboardDataAsync();
            }
            else
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                data = string.IsNullOrEmpty(userId)
                    ? await _dashboardService.GetDashboardDataAsync()
                    : await _dashboardService.GetDashboardDataForUserAsync(userId);
            }

            var recentSessions = data.AllSessions
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                Statistics = data,
                SelectedSessionId = sessionId,
                IsAdmin = isAdmin,
                RecentSessions = recentSessions
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard for session {SessionId}", sessionId);
            TempData["Error"] = "An error occurred while loading the dashboard.";
            return View(new DashboardViewModel
            {
                Statistics = new DashboardDto(),
                SelectedSessionId = sessionId
            });
        }
    }
}
