using System.Security.Claims;
using ERDRR.Models.DTOs;
using ERDRR.Models.Entities;
using ERDRR.Models.ViewModels;
using ERDRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERDRR.Controllers;

[Authorize]
public class SessionController : Controller
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var model = await _sessionService.GetSessionsPagedAsync(page, 10, search);
        return View(model);
    }

    public IActionResult Create()
    {
        return View(new SessionCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SessionCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var dto = new CreateSessionDto
        {
            Name = model.Name,
            Description = model.Description,
            AlgorithmType = model.AlgorithmType,
            TimeQuantum = model.TimeQuantum,
            IsPreemptive = model.IsPreemptive
        };

        await _sessionService.CreateAsync(dto, userId ?? string.Empty);
        TempData["Success"] = "Session created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var session = await _sessionService.GetByIdAsync(id);
        if (session == null) return NotFound();

        var model = new SessionEditViewModel
        {
            Id = session.Id,
            Name = session.Name,
            Description = session.Description,
            AlgorithmType = session.AlgorithmType,
            TimeQuantum = session.TimeQuantum,
            IsPreemptive = session.IsPreemptive,
            Status = session.Status
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SessionEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _sessionService.UpdateAsync(model.Id, model);
        TempData["Success"] = "Session updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _sessionService.DeleteAsync(id);
        TempData["Success"] = "Session deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
