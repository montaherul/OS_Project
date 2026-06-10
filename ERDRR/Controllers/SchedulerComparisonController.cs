using System.Security.Claims;
using ERDRR.Models.DTOs;
using ERDRR.Models.ViewModels;
using ERDRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERDRR.Controllers;

[Authorize]
public class SchedulerComparisonController : Controller
{
    private readonly IComparisonService _comparisonService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<SchedulerComparisonController> _logger;

    public SchedulerComparisonController(
        IComparisonService comparisonService,
        ISessionService sessionService,
        ILogger<SchedulerComparisonController> logger)
    {
        _comparisonService = comparisonService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? sessionId)
    {
        try
        {
            var sessions = (await _sessionService.GetSessionsPagedAsync(1, 100)).Sessions;

            var viewModel = new SchedulerComparisonViewModel
            {
                Sessions = sessions,
                SelectedSessionId = sessionId
            };

            if (sessionId.HasValue)
            {
                viewModel.Result = await _comparisonService.GetLatestComparisonAsync(sessionId.Value);
            }

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comparison page");
            TempData["Error"] = "An error occurred while loading the comparison page.";
            return View(new SchedulerComparisonViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunComparison(int sessionId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _comparisonService.CompareAlgorithmsAsync(sessionId, userId);
            TempData["Success"] = "Comparison completed successfully!";
            return RedirectToAction(nameof(Index), new { sessionId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index), new { sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running comparison for session {SessionId}", sessionId);
            TempData["Error"] = "An error occurred while running the comparison.";
            return RedirectToAction(nameof(Index), new { sessionId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetComparisonChartData(int comparisonId)
    {
        var chartData = await _comparisonService.GetChartDataAsync(comparisonId);
        return Json(chartData);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportComparisonReport(int comparisonId, string format)
    {
        try
        {
            var exportData = await _comparisonService.GetExportDataAsync(comparisonId);

            if (format.ToLower() == "excel")
            {
                return GenerateExcelExport(exportData);
            }

            return RedirectToAction(nameof(Index), new { sessionId = exportData.SessionName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting comparison {ComparisonId}", comparisonId);
            TempData["Error"] = "An error occurred while exporting the report.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int sessionId)
    {
        try
        {
            await _comparisonService.DeleteComparisonAsync(id);
            TempData["Success"] = "Comparison deleted.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comparison {Id}", id);
            TempData["Error"] = "An error occurred while deleting the comparison.";
        }
        return RedirectToAction(nameof(Index), new { sessionId });
    }

    private IActionResult GenerateExcelExport(ComparisonExportDto data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Scheduler Comparison Report");
        sb.AppendLine($"Session: {data.SessionName}");
        sb.AppendLine($"Processes: {data.ProcessCount}");
        sb.AppendLine($"Generated: {data.GeneratedAt:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Recommended: {data.RecommendedAlgorithm}");
        sb.AppendLine();
        sb.AppendLine("Metric,EDF,RR,Hybrid,Best");

        foreach (var row in data.Metrics)
        {
            sb.AppendLine($"{row.MetricName},{row.EDFValue},{row.RRValue},{row.HybridValue},{row.BestAlgorithm}");
        }

        sb.AppendLine();
        sb.AppendLine($"Recommendation: {data.RecommendationReason}");

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"Comparison_{data.SessionName}_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
