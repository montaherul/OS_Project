using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApiController : ControllerBase
{
    private readonly IProcessService _processService;
    private readonly ISessionService _sessionService;
    private readonly ISimulationService _simulationService;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<ApiController> _logger;

    public ApiController(
        IProcessService processService,
        ISessionService sessionService,
        ISimulationService simulationService,
        IDashboardService dashboardService,
        ILogger<ApiController> logger)
    {
        _processService = processService;
        _sessionService = sessionService;
        _simulationService = simulationService;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var data = await _dashboardService.GetDashboardDataAsync();
        return Ok(data);
    }

    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSession(int id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session == null) return NotFound();
        return Ok(session);
    }

    [HttpGet("processes/{sessionId}")]
    public async Task<IActionResult> GetProcesses(int sessionId)
    {
        var processes = await _processService.GetBySessionIdAsync(sessionId);
        return Ok(processes);
    }

    [HttpPost("simulation/run/{sessionId}")]
    public async Task<IActionResult> RunSimulation(int sessionId)
    {
        try
        {
            var result = await _simulationService.ExecuteFullSimulationAsync(sessionId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("simulation/step")]
    public async Task<IActionResult> RunStep([FromBody] StepRequest request)
    {
        try
        {
            var step = await _simulationService.ExecuteStepAsync(request.SessionId, request.TimeStep);
            return Ok(step);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class StepRequest
{
    public int SessionId { get; set; }
    public int TimeStep { get; set; }
}
