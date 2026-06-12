using System.Security.Claims;
using EDFRR.Models.DTOs;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ProcessManagementController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ILogger<ProcessManagementController> _logger;

    public ProcessManagementController(IAdminService adminService, ILogger<ProcessManagementController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetProcesses(
        string? search, string? userFilter, string? statusFilter,
        DateTime? dateFrom, DateTime? dateTo,
        string sortColumn = "createdAt", string sortDirection = "desc",
        int draw = 1, int start = 0, int length = 10)
    {
        try
        {
            var pageNumber = (start / length) + 1;
            var mappedSort = MapSortColumn(sortColumn);
            var mappedDir = sortDirection.ToUpper() == "ASC" ? "ASC" : "DESC";

            var data = await _adminService.GetProcessesPagedAsync(
                pageNumber, length, search, mappedSort, mappedDir, userFilter, statusFilter, dateFrom, dateTo);

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
            _logger.LogError(ex, "Error fetching processes");
            return Json(new { draw, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<AdminProcessListDto>() });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFilterOptions()
    {
        var users = await _adminService.GetProcessUserOptionsAsync();
        var statuses = await _adminService.GetProcessStatusOptionsAsync();
        return Json(new { users, statuses });
    }

    public async Task<IActionResult> Details(int id)
    {
        var process = await _adminService.GetProcessDetailsAsync(id);
        if (process == null) return NotFound();
        return View(process);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var result = await _adminService.DeleteProcessAsync(id, adminId);

        if (IsAjax()) return Json(new { success = result, message = result ? "Process deleted." : "Failed to delete process." });

        if (result) TempData["Success"] = "Process deleted."; else TempData["Error"] = "Failed to delete process.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAction(string action, List<int> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
        {
            if (IsAjax()) return Json(new { success = false, message = "No processes selected." });
            TempData["Error"] = "No processes selected.";
            return RedirectToAction(nameof(Index));
        }

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        switch (action)
        {
            case "delete":
                await _adminService.BulkDeleteProcessesAsync(selectedIds, adminId);
                break;
            default:
                if (IsAjax()) return Json(new { success = false, message = "Invalid action." });
                TempData["Error"] = "Invalid action.";
                return RedirectToAction(nameof(Index));
        }

        if (IsAjax()) return Json(new { success = true, message = $"{selectedIds.Count} processes deleted." });

        TempData["Success"] = $"{selectedIds.Count} processes deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? searchTerm, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var bytes = await _adminService.ExportProcessesToExcelAsync(searchTerm, userFilter, statusFilter, dateFrom, dateTo);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Processes_Export_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? searchTerm, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var bytes = await _adminService.ExportProcessesToPdfAsync(searchTerm, userFilter, statusFilter, dateFrom, dateTo);
        return File(bytes, "application/pdf", $"Processes_Export_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    private string MapSortColumn(string column)
    {
        return column.ToLower() switch
        {
            "processname" => "ProcessName",
            "arrivaltime" => "ArrivalTime",
            "bursttime" => "BurstTime",
            "deadline" => "Deadline",
            "priority" => "Priority",
            "status" => "Status",
            "createdat" => "CreatedAt",
            _ => "CreatedAt"
        };
    }

    private bool IsAjax() =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
        Request.Headers["Accept"].ToString().Contains("application/json");
}
