using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ActivityLogsController : Controller
{
    private readonly IActivityLogService _activityLogService;
    private readonly ILogger<ActivityLogsController> _logger;

    public ActivityLogsController(IActivityLogService activityLogService, ILogger<ActivityLogsController> logger)
    {
        _activityLogService = activityLogService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var logs = await _activityLogService.GetRecentAsync(100);
            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading activity logs");
            TempData["Error"] = "Error loading activity logs.";
            return View(new List<Models.DTOs.ActivityLogDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent(int count = 20)
    {
        try
        {
            var logs = await _activityLogService.GetRecentAsync(count);
            return Json(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent activities");
            return Json(Array.Empty<Models.DTOs.ActivityLogDto>());
        }
    }
}
