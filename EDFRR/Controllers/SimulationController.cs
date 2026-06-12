using EDFRR.Models.DTOs;
using EDFRR.Models.ViewModels;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Controllers;

[Authorize]
public class SimulationController : Controller
{
    private readonly ISimulationService _simulationService;
    private readonly ISessionRepository _sessionRepository;
    private readonly ILogger<SimulationController> _logger;

    public SimulationController(
        ISimulationService simulationService,
        ISessionRepository sessionRepository,
        ILogger<SimulationController> logger)
    {
        _simulationService = simulationService;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int sessionId)
    {
        try
        {
            var model = await _simulationService.InitializeSimulationAsync(sessionId);
            return View(model);
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = "Session not found.";
            return RedirectToAction("Index", "Session", new { area = "" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RunFull(int sessionId)
    {
        try
        {
            var result = await _simulationService.ExecuteFullSimulationAsync(sessionId);
            var model = await _simulationService.InitializeSimulationAsync(sessionId);
            model.CurrentStep = result;
            model.FinalMetrics = result.CurrentMetrics;
            model.GanttChart = result.GanttEntries;
            model.ExecutionLogs = result.ProcessStates != null
                ? await LoadExecutionLogsAsync(sessionId)
                : new List<ExecutionLogDto>();
            model.IsComplete = true;
            model.IsRunning = false;
            return View("Index", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running simulation");
            TempData["Error"] = $"Error running simulation: {ex.Message}";
            return RedirectToAction(nameof(Index), new { sessionId });
        }
    }

    private async Task<List<ExecutionLogDto>> LoadExecutionLogsAsync(int sessionId)
    {
        try
        {
            var logs = await _simulationService.GetExecutionLogsAsync(sessionId);
            return logs;
        }
        catch
        {
            return new List<ExecutionLogDto>();
        }
    }

    [HttpPost]
    public async Task<IActionResult> RunStep(int sessionId, int currentTimeStep)
    {
        try
        {
            var step = await _simulationService.ExecuteStepAsync(sessionId, currentTimeStep);
            var model = await _simulationService.InitializeSimulationAsync(sessionId);
            model.CurrentStep = step;
            model.IsRunning = true;
            model.IsPaused = true;
            model.GanttChart = step.GanttEntries;
            return Json(new
            {
                success = true,
                data = step
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing step");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Reset(int sessionId)
    {
        var model = await _simulationService.InitializeSimulationAsync(sessionId);
        return View("Index", model);
    }
}
