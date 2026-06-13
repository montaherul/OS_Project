using EDFRR.Models.DTOs;
using EDFRR.Models.ViewModels;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IReportService reportService,
        ISessionService sessionService,
        ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? sessionId)
    {
        var sessionResult = await _sessionService.GetSessionsPagedAsync(1, 100);
        var model = new ReportViewModel
        {
            Sessions = sessionResult.Sessions
        };

        if (sessionId.HasValue)
        {
            try
            {
                model.Report = await _reportService.GenerateReportAsync(sessionId.Value);
                model.SelectedSessionId = sessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading report for session {SessionId}", sessionId);
                TempData["Warning"] = $"Could not load report: {ex.Message}";
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Export(int sessionId, string format)
    {
        try
        {
            if (format?.ToLower() == "pdf")
            {
                var pdfBytes = await _reportService.GeneratePdfReportAsync(sessionId);
                return File(pdfBytes, "application/pdf", $"SchedulingReport_{sessionId}.pdf");
            }
            else
            {
                var excelBytes = await _reportService.GenerateExcelReportAsync(sessionId);
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"SchedulingReport_{sessionId}.xlsx");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting report for session {SessionId}", sessionId);
            TempData["Error"] = $"Error exporting report: {ex.Message}";
            return RedirectToAction(nameof(Index), new { sessionId });
        }
    }
}
