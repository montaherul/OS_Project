using System.Security.Claims;
using EDFRR.Models.DTOs;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SessionManagementController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ILogger<SessionManagementController> _logger;

    public SessionManagementController(IAdminService adminService, ILogger<SessionManagementController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetSessions(
        string? search, string? algorithmFilter, string? userFilter,
        DateTime? dateFrom, DateTime? dateTo,
        string sortColumn = "createdAt", string sortDirection = "desc",
        int draw = 1, int start = 0, int length = 10)
    {
        try
        {
            var pageNumber = (start / length) + 1;
            var mappedSort = MapSortColumn(sortColumn);
            var mappedDir = sortDirection.ToUpper() == "ASC" ? "ASC" : "DESC";

            var data = await _adminService.GetSessionsPagedAsync(
                pageNumber, length, search, mappedSort, mappedDir, algorithmFilter, userFilter, dateFrom, dateTo);

            return Json(new
            {
                draw,
                recordsTotal = data.TotalCount,
                recordsFiltered = data.TotalCount,
                data = data.Items
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching sessions");
            return Json(new { draw, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<AdminSessionListDto>() });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFilterOptions()
    {
        var users = await _adminService.GetSessionUserOptionsAsync();
        var algorithms = await _adminService.GetAlgorithmOptionsAsync();
        return Json(new { users, algorithms });
    }

    public async Task<IActionResult> Details(int id)
    {
        var session = await _adminService.GetSessionDetailsAsync(id);
        if (session == null) return NotFound();
        return View(session);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var result = await _adminService.DeleteSessionAsync(id, adminId);

        if (IsAjax()) return Json(new { success = result, message = result ? "Session deleted." : "Failed to delete session." });

        if (result) TempData["Success"] = "Session deleted."; else TempData["Error"] = "Failed to delete session.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAction(string action, List<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
        {
            if (IsAjax()) return Json(new { success = false, message = "No sessions selected." });
            TempData["Error"] = "No sessions selected.";
            return RedirectToAction(nameof(Index));
        }

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        switch (action)
        {
            case "delete":
                await _adminService.BulkDeleteSessionsAsync(selectedIds, adminId);
                break;
            default:
                if (IsAjax()) return Json(new { success = false, message = "Invalid action." });
                TempData["Error"] = "Invalid action.";
                return RedirectToAction(nameof(Index));
        }

        if (IsAjax()) return Json(new { success = true, message = $"{selectedIds.Count} sessions deleted." });

        TempData["Success"] = $"{selectedIds.Count} sessions deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? searchTerm, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var bytes = await _adminService.ExportSessionsToExcelAsync(searchTerm, algorithmFilter, userFilter, dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Sessions_Export_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? searchTerm, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var bytes = await _adminService.ExportSessionsToPdfAsync(searchTerm, algorithmFilter, userFilter, dateFrom, dateTo);
        return File(bytes, "application/pdf", $"Sessions_Export_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private string MapSortColumn(string column)
    {
        return column.ToLower() switch
        {
            "sessionname" => "SessionName",
            "algorithmtype" => "AlgorithmType",
            "processcount" => "ProcessCount",
            "status" => "Status",
            "createdat" => "CreatedAt",
            _ => "CreatedAt"
        };
    }

    private bool IsAjax() =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
        Request.Headers["Accept"].ToString().Contains("application/json");
}
