using System.Security.Claims;
using EDFRR.Models.DTOs;
using EDFRR.Models.ViewModels;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDFRR.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly IAdminService _adminService;
    private readonly IAdminRepository _adminRepository;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(IAdminService adminService, IAdminRepository adminRepository, ILogger<UserManagementController> logger)
    {
        _adminService = adminService;
        _adminRepository = adminRepository;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View(new UserManagementViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(
        string? search, string? roleFilter, string? statusFilter,
        string sortColumn = "createdAt", string sortDirection = "desc",
        int draw = 1, int start = 0, int length = 10)
    {
        try
        {
            var pageNumber = (start / length) + 1;
            var users = await _adminRepository.GetUsersPagedAsync(
                pageNumber, length, search, sortColumn, sortDirection, roleFilter, statusFilter);

            return Json(new
            {
                draw,
                recordsTotal = users.TotalCount,
                recordsFiltered = users.TotalCount,
                data = users.Items
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users via AJAX");
            return Json(new { draw, recordsTotal = 0, recordsFiltered = 0, data = Array.Empty<AdminUserDto>() });
        }
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        var user = await _adminService.GetUserDetailsAsync(id);
        if (user == null) return NotFound();

        return View(new UserDetailsViewModel { User = user });
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        var user = await _adminService.GetUserDetailsAsync(id);
        if (user == null) return NotFound();

        var model = new EditUserViewModel
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CurrentRole = user.Roles
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _adminService.UpdateUserAsync(
            model.UserId, model.FirstName, model.LastName, model.Email, model.PhoneNumber, model.IsActive);

        if (result)
            TempData["Success"] = "User updated successfully.";
        else
            TempData["Error"] = "Failed to update user.";

        return RedirectToAction(nameof(Details), new { id = model.UserId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var result = await _adminService.DeleteUserAsync(id, adminId);

        if (IsAjax()) return Json(new { success = result, message = result ? "User deleted." : "Failed to delete user." });

        if (result) TempData["Success"] = "User deleted."; else TempData["Error"] = "Failed to delete user.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var result = await _adminService.LockUserAsync(id, adminId);

        if (IsAjax()) return Json(new { success = result, message = result ? "User locked." : "Failed to lock user." });

        if (result) TempData["Success"] = "User locked."; else TempData["Error"] = "Failed to lock user.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(string id)
    {
        if (string.IsNullOrEmpty(id)) return BadRequest();

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var result = await _adminService.UnlockUserAsync(id, adminId);

        if (IsAjax()) return Json(new { success = result, message = result ? "User unlocked." : "Failed to unlock user." });

        if (result) TempData["Success"] = "User unlocked."; else TempData["Error"] = "Failed to unlock user.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newPassword))
            return BadRequest();

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var result = await _adminService.ResetPasswordAsync(id, newPassword, adminId);

        if (IsAjax()) return Json(new { success = result, message = result ? "Password reset successfully." : "Failed to reset password." });

        if (result) TempData["Success"] = "Password reset successfully."; else TempData["Error"] = "Failed to reset password.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role)) return BadRequest();

        var result = await _adminService.AssignRoleAsync(userId, role);
        var msg = result ? $"Role '{role}' assigned." : "Failed to assign role.";

        if (IsAjax()) return Json(new { success = result, message = msg });

        TempData["Success"] = msg;
        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRole(string userId, string role)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role)) return BadRequest();

        var result = await _adminService.RemoveRoleAsync(userId, role);
        var msg = result ? $"Role '{role}' removed." : "Failed to remove role.";

        if (IsAjax()) return Json(new { success = result, message = msg });

        TempData["Success"] = msg;
        return RedirectToAction(nameof(Details), new { id = userId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkAction(string action, List<string> selectedIds)
    {
        if (selectedIds == null || !selectedIds.Any())
        {
            if (IsAjax()) return Json(new { success = false, message = "No users selected." });
            TempData["Error"] = "No users selected.";
            return RedirectToAction(nameof(Index));
        }

        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var message = "";

        switch (action)
        {
            case "delete":
                await _adminService.BulkDeleteUsersAsync(selectedIds);
                message = $"{selectedIds.Count} users deleted.";
                break;
            case "lock":
                await _adminService.BulkLockUsersAsync(selectedIds);
                message = $"{selectedIds.Count} users locked.";
                break;
            case "unlock":
                await _adminService.BulkUnlockUsersAsync(selectedIds);
                message = $"{selectedIds.Count} users unlocked.";
                break;
            default:
                if (IsAjax()) return Json(new { success = false, message = "Invalid action." });
                TempData["Error"] = "Invalid action.";
                return RedirectToAction(nameof(Index));
        }

        if (IsAjax()) return Json(new { success = true, message });

        TempData["Success"] = message;
        return RedirectToAction(nameof(Index));
    }

    private bool IsAjax() =>
        Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
        Request.Headers["Accept"].ToString().Contains("application/json");

    [HttpGet]
    public async Task<IActionResult> ExportExcel(string? searchTerm, string? roleFilter, string? statusFilter)
    {
        var bytes = await _adminService.ExportUsersToExcelAsync(searchTerm, roleFilter, statusFilter);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Users_Export_{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(string? searchTerm, string? roleFilter, string? statusFilter)
    {
        var bytes = await _adminService.ExportUsersToPdfAsync(searchTerm, roleFilter, statusFilter);
        return File(bytes, "application/pdf", $"Users_Export_{DateTime.UtcNow:yyyyMMdd}.pdf");
    }
}
