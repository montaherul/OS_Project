using ERDRR.Models.ViewModels;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERDRR.Controllers;

[Authorize]
public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IReportService reportService,
        ISessionRepository sessionRepository,
        ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var sessions = await _sessionRepository.GetSessionsPagedAsync(1, 100);
        var model = new ReportViewModel
        {
            Sessions = sessions.Select(s => new Models.DTOs.SessionDto
            {
                Id = s.Id,
                Name = s.Name,
                AlgorithmType = s.AlgorithmType,
                Status = s.Status
            }).ToList()
        };
        return View(model);
    }

    public async Task<IActionResult> ViewReport(int sessionId)
    {
        try
        {
            var report = await _reportService.GenerateReportAsync(sessionId);
            var sessions = await _sessionRepository.GetSessionsPagedAsync(1, 100);
            var model = new ReportViewModel
            {
                Sessions = sessions.Select(s => new Models.DTOs.SessionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    AlgorithmType = s.AlgorithmType,
                    Status = s.Status
                }).ToList(),
                SelectedSessionId = sessionId,
                Report = report
            };
            return View("Index", model);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error generating report: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> DownloadPdf(int sessionId)
    {
        try
        {
            var pdfBytes = await _reportService.GeneratePdfReportAsync(sessionId);
            return File(pdfBytes, "application/pdf", $"SchedulingReport_{sessionId}.pdf");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error generating PDF: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> DownloadExcel(int sessionId)
    {
        try
        {
            var excelBytes = await _reportService.GenerateExcelReportAsync(sessionId);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SchedulingReport_{sessionId}.xlsx");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error generating Excel: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
