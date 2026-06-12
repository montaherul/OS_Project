using System.Text.Json;
using EDFRR.Models.DTOs;
using EDFRR.Models.Entities;
using EDFRR.Models.ViewModels;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Controllers;

[Authorize]
public class ProcessController : Controller
{
    private readonly IProcessService _processService;
    private readonly IProcessImportService _processImportService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<ProcessController> _logger;

    public ProcessController(
        IProcessService processService,
        IProcessImportService processImportService,
        ISessionRepository sessionRepository,
        ILogger<ProcessController> logger)
    {
        _processService = processService;
        _processImportService = processImportService;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int sessionId, int page = 1, string? search = null, string? status = null)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return NotFound();

        var model = await _processService.GetProcessesPagedAsync(sessionId, page, 10, search, status);
        model.SessionName = session.Name;
        return View(model);
    }

    public async Task<IActionResult> Create(int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return NotFound();

        var model = new ProcessCreateViewModel
        {
            SchedulingSessionId = sessionId,
            SessionName = session.Name
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProcessCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var dto = new CreateProcessDto
        {
            Name = model.Name,
            ArrivalTime = model.ArrivalTime,
            BurstTime = model.BurstTime,
            Deadline = model.Deadline,
            Priority = model.Priority,
            SchedulingSessionId = model.SchedulingSessionId
        };

        await _processService.CreateAsync(dto, userId ?? string.Empty);
        TempData["Success"] = "Process created successfully.";
        return RedirectToAction(nameof(Index), new { sessionId = model.SchedulingSessionId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var process = await _processService.GetByIdAsync(id);
        if (process == null) return NotFound();

        var session = await _sessionRepository.GetByIdAsync(process.SchedulingSessionId);
        var model = new ProcessEditViewModel
        {
            Id = process.Id,
            Name = process.Name,
            ProcessId = process.ProcessId,
            ArrivalTime = process.ArrivalTime,
            BurstTime = process.BurstTime,
            Deadline = process.Deadline,
            Priority = process.Priority,
            SchedulingSessionId = process.SchedulingSessionId,
            SessionName = session?.Name ?? string.Empty
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProcessEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _processService.UpdateAsync(model.Id, model);
        TempData["Success"] = "Process updated successfully.";
        return RedirectToAction(nameof(Index), new { sessionId = model.SchedulingSessionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int sessionId)
    {
        await _processService.DeleteAsync(id);
        TempData["Success"] = "Process deleted successfully.";
        return RedirectToAction(nameof(Index), new { sessionId });
    }

    [HttpPost]
    public async Task<IActionResult> BulkAdd(int sessionId, int count)
    {
        var random = new Random();
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        for (int i = 0; i < count; i++)
        {
            var dto = new CreateProcessDto
            {
                Name = $"Process_{i + 1}",
                ArrivalTime = random.Next(0, 10),
                BurstTime = random.Next(1, 10),
                Deadline = random.Next(5, 20),
                Priority = random.Next(0, 5),
                SchedulingSessionId = sessionId
            };
            await _processService.CreateAsync(dto, userId ?? string.Empty);
        }

        TempData["Success"] = $"{count} processes added successfully.";
        return RedirectToAction(nameof(Index), new { sessionId });
    }

    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    //  IMPORT ACTIONS
    // â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>
    /// GET: Displays the import page for a given session.
    /// </summary>
    public async Task<IActionResult> Import(int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return NotFound();

        var model = new ImportProcessViewModel
        {
            SchedulingSessionId = sessionId,
            SessionName = session.Name
        };
        return View(model);
    }

    /// <summary>
    /// POST: Processes the uploaded file, validates rows, saves valid ones.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ImportProcessViewModel model)
    {
        var session = await _sessionRepository.GetByIdAsync(model.SchedulingSessionId);
        if (session == null) return NotFound();

        model.SessionName = session.Name;

        if (model.File == null || model.File.Length == 0)
        {
            ModelState.AddModelError("File", "Please select a file to upload.");
            return View(model);
        }

        var extension = Path.GetExtension(model.File.FileName).ToLowerInvariant();
        if (extension != ".xlsx" && extension != ".csv")
        {
            ModelState.AddModelError("File", "Only .xlsx and .csv files are supported.");
            return View(model);
        }

        try
        {
            List<ProcessImportRow> rows;

            using (var stream = model.File.OpenReadStream())
            {
                rows = extension == ".xlsx"
                    ? await _processImportService.ParseExcelAsync(stream)
                    : await _processImportService.ParseCsvAsync(stream);
            }

            if (rows.Count == 0)
            {
                ModelState.AddModelError("File", "The file contains no data rows.");
                return View(model);
            }

            var errors = _processImportService.ValidateRows(rows);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var result = await _processImportService.SaveValidRowsAsync(
                rows, errors, model.SchedulingSessionId, model.SessionName, userId);

            TempData["ImportResult"] = JsonSerializer.Serialize(result);
            TempData["Success"] = $"Import complete: {result.ImportedRows} imported, {result.FailedRows} failed.";
            return RedirectToAction(nameof(ImportResults), new { sessionId = model.SchedulingSessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing process file");
            ModelState.AddModelError("File", $"Error processing file: {ex.Message}");
            return View(model);
        }
    }

    /// <summary>
    /// GET: Shows the import results summary page.
    /// </summary>
    public async Task<IActionResult> ImportResults(int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return NotFound();

        var resultJson = TempData["ImportResult"] as string;
        if (string.IsNullOrEmpty(resultJson))
        {
            TempData["Error"] = "No import results found. Please import a file first.";
            return RedirectToAction(nameof(Import), new { sessionId });
        }

        var result = JsonSerializer.Deserialize<ProcessImportResult>(resultJson);
        if (result == null)
        {
            TempData["Error"] = "Import results could not be loaded.";
            return RedirectToAction(nameof(Import), new { sessionId });
        }

        result.SchedulingSessionId = sessionId;
        result.SessionName = session.Name;

        return View(result);
    }

    /// <summary>
    /// GET: Downloads a sample CSV file.
    /// </summary>
    public IActionResult DownloadSampleCsv()
    {
        var bytes = _processImportService.GenerateSampleCsv();
        return File(bytes, "text/csv", "sample_processes.csv");
    }

    /// <summary>
    /// GET: Downloads a sample Excel file.
    /// </summary>
    public IActionResult DownloadSampleExcel()
    {
        var bytes = _processImportService.GenerateSampleExcel();
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "sample_processes.xlsx");
    }
}
