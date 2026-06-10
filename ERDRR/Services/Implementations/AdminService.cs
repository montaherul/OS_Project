using System.Text;
using ClosedXML.Excel;
using ERDRR.Models.DTOs;
using ERDRR.Models.Entities;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Identity;

namespace ERDRR.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminService> _logger;
    private readonly IActivityLogService _activityLogService;

    public AdminService(
        IAdminRepository adminRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminService> logger,
        IActivityLogService activityLogService)
    {
        _adminRepository = adminRepository;
        _userManager = userManager;
        _logger = logger;
        _activityLogService = activityLogService;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync()
    {
        return await _adminRepository.GetDashboardStatsAsync();
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? roleFilter, string? statusFilter)
    {
        return await _adminRepository.GetUsersPagedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortDirection, roleFilter, statusFilter);
    }

    public async Task<AdminUserDetailDto?> GetUserDetailsAsync(string userId)
    {
        return await _adminRepository.GetUserDetailsAsync(userId);
    }

    public async Task<bool> UpdateUserAsync(string userId, string firstName, string lastName, string email, string? phoneNumber, bool isActive)
    {
        var result = await _adminRepository.UpdateUserAsync(userId, firstName, lastName, email, phoneNumber, isActive);
        if (result)
            _logger.LogInformation("Admin updated user {UserId}", userId);
        return result;
    }

    public async Task<bool> DeleteUserAsync(string userId, string performedBy)
    {
        var result = await _adminRepository.DeleteUserAsync(userId);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} deleted user {UserId}", performedBy, userId);
            await _activityLogService.LogAsync(performedBy, "User Deleted", $"User {userId} was deleted", null);
        }
        return result;
    }

    public async Task<bool> LockUserAsync(string userId, string performedBy)
    {
        var result = await _adminRepository.LockUserAsync(userId);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} locked user {UserId}", performedBy, userId);
            await _activityLogService.LogAsync(performedBy, "User Locked", $"User {userId} was locked", null);
        }
        return result;
    }

    public async Task<bool> UnlockUserAsync(string userId, string performedBy)
    {
        var result = await _adminRepository.UnlockUserAsync(userId);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} unlocked user {UserId}", performedBy, userId);
            await _activityLogService.LogAsync(performedBy, "User Unlocked", $"User {userId} was unlocked", null);
        }
        return result;
    }

    public async Task<bool> ResetPasswordAsync(string userId, string newPassword, string performedBy)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
            _logger.LogInformation("Admin {AdminId} reset password for user {UserId}", performedBy, userId);
        else
            _logger.LogWarning("Admin {AdminId} failed to reset password for user {UserId}: {Errors}",
                performedBy, userId, string.Join(", ", result.Errors.Select(e => e.Description)));

        return result.Succeeded;
    }

    public async Task<bool> AssignRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, role))
            return true;

        var result = await _userManager.AddToRoleAsync(user, role);
        if (result.Succeeded)
        {
            _logger.LogInformation("Assigned role {Role} to user {UserId}", role, userId);
            await _activityLogService.LogAsync(null, "Role Changed", $"Role '{role}' assigned to user {userId}", null);
        }
        return result.Succeeded;
    }

    public async Task<bool> RemoveRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.RemoveFromRoleAsync(user, role);
        if (result.Succeeded)
        {
            _logger.LogInformation("Removed role {Role} from user {UserId}", role, userId);
            await _activityLogService.LogAsync(null, "Role Changed", $"Role '{role}' removed from user {userId}", null);
        }
        return result.Succeeded;
    }

    public async Task<bool> BulkDeleteUsersAsync(List<string> userIds)
    {
        var result = await _adminRepository.BulkDeleteUsersAsync(userIds);
        if (result)
            await _activityLogService.LogAsync(null, "User Deleted", $"{userIds.Count} users were deleted", null);
        return result;
    }

    public async Task<bool> BulkLockUsersAsync(List<string> userIds)
    {
        var result = await _adminRepository.BulkLockUsersAsync(userIds);
        if (result)
            await _activityLogService.LogAsync(null, "User Locked", $"{userIds.Count} users were locked", null);
        return result;
    }

    public async Task<bool> BulkUnlockUsersAsync(List<string> userIds)
    {
        var result = await _adminRepository.BulkUnlockUsersAsync(userIds);
        if (result)
            await _activityLogService.LogAsync(null, "User Unlocked", $"{userIds.Count} users were unlocked", null);
        return result;
    }

    public async Task<byte[]> ExportUsersToExcelAsync(string? searchTerm, string? roleFilter, string? statusFilter)
    {
        var users = await _adminRepository.GetUsersPagedAsync(1, 10000, searchTerm, "CreatedAt", "DESC", roleFilter, statusFilter);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Users");

        sheet.Cell(1, 1).Value = "User ID";
        sheet.Cell(1, 2).Value = "Username";
        sheet.Cell(1, 3).Value = "Email";
        sheet.Cell(1, 4).Value = "First Name";
        sheet.Cell(1, 5).Value = "Last Name";
        sheet.Cell(1, 6).Value = "Roles";
        sheet.Cell(1, 7).Value = "Status";
        sheet.Cell(1, 8).Value = "Created Date";
        sheet.Cell(1, 9).Value = "Last Login";

        var headerRow = sheet.Row(1);
        foreach (var cell in headerRow.Cells())
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var user in users.Items)
        {
            sheet.Cell(row, 1).Value = user.UserId;
            sheet.Cell(row, 2).Value = user.UserName;
            sheet.Cell(row, 3).Value = user.Email;
            sheet.Cell(row, 4).Value = user.FirstName;
            sheet.Cell(row, 5).Value = user.LastName;
            sheet.Cell(row, 6).Value = user.Roles;
            sheet.Cell(row, 7).Value = user.Status;
            sheet.Cell(row, 8).Value = user.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            sheet.Cell(row, 9).Value = user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm") ?? "Never";
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportUsersToPdfAsync(string? searchTerm, string? roleFilter, string? statusFilter)
    {
        var users = await _adminRepository.GetUsersPagedAsync(1, 10000, searchTerm, "CreatedAt", "DESC", roleFilter, statusFilter);

        using var stream = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate(), 15, 15, 15, 15);
        var writer = PdfWriter.GetInstance(document, stream);
        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

        document.Add(new Paragraph("ERDRR - User Management Report", titleFont));
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}", normalFont));
        document.Add(new Paragraph(" "));

        var table = new PdfPTable(7);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 15, 15, 20, 12, 12, 10, 16 });

        string[] headers = { "Username", "Email", "Name", "Roles", "Status", "Created", "Last Login" };
        foreach (var h in headers)
        {
            var cell = new Phrase(h, headerFont);
            var pdfCell = new PdfPCell(cell);
            pdfCell.BackgroundColor = new BaseColor(33, 37, 41);
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Padding = 5;
            table.AddCell(pdfCell);
        }

        foreach (var user in users.Items)
        {
            table.AddCell(new Phrase(user.UserName, normalFont));
            table.AddCell(new Phrase(user.Email, normalFont));
            table.AddCell(new Phrase($"{user.FirstName} {user.LastName}", normalFont));
            table.AddCell(new Phrase(user.Roles, normalFont));
            table.AddCell(new Phrase(user.Status, normalFont));
            table.AddCell(new Phrase(user.CreatedAt.ToString("yyyy-MM-dd"), normalFont));
            table.AddCell(new Phrase(user.LastLoginAt?.ToString("yyyy-MM-dd") ?? "Never", normalFont));
        }

        document.Add(table);
        document.Close();

        return stream.ToArray();
    }

    public async Task<PagedResult<AdminProcessListDto>> GetProcessesPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        return await _adminRepository.GetProcessesPagedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortDirection, userFilter, statusFilter, dateFrom, dateTo);
    }

    public async Task<AdminProcessDetailDto?> GetProcessDetailsAsync(int processId)
    {
        return await _adminRepository.GetProcessDetailsAsync(processId);
    }

    public async Task<bool> DeleteProcessAsync(int processId, string performedBy)
    {
        var result = await _adminRepository.DeleteProcessAsync(processId);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} deleted process {ProcessId}", performedBy, processId);
            await _activityLogService.LogAsync(performedBy, "Process Deleted", $"Process ID {processId} was deleted", null);
        }
        return result;
    }

    public async Task<bool> BulkDeleteProcessesAsync(List<int> processIds, string performedBy)
    {
        var result = await _adminRepository.BulkDeleteProcessesAsync(processIds);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} bulk deleted {Count} processes", performedBy, processIds.Count);
            await _activityLogService.LogAsync(performedBy, "Process Deleted", $"{processIds.Count} processes were deleted", null);
        }
        return result;
    }

    public async Task<PagedResult<AdminSessionListDto>> GetSessionsPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        return await _adminRepository.GetSessionsPagedAsync(pageNumber, pageSize, searchTerm, sortColumn, sortDirection, algorithmFilter, userFilter, dateFrom, dateTo);
    }

    public async Task<AdminSessionDetailDto?> GetSessionDetailsAsync(int sessionId)
    {
        return await _adminRepository.GetSessionDetailsAsync(sessionId);
    }

    public async Task<bool> DeleteSessionAsync(int sessionId, string performedBy)
    {
        var result = await _adminRepository.DeleteSessionAsync(sessionId);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} deleted session {SessionId}", performedBy, sessionId);
            await _activityLogService.LogAsync(performedBy, "Session Deleted", $"Session ID {sessionId} was deleted", null);
        }
        return result;
    }

    public async Task<bool> BulkDeleteSessionsAsync(List<int> sessionIds, string performedBy)
    {
        var result = await _adminRepository.BulkDeleteSessionsAsync(sessionIds);
        if (result)
        {
            _logger.LogInformation("Admin {AdminId} bulk deleted {Count} sessions", performedBy, sessionIds.Count);
            await _activityLogService.LogAsync(performedBy, "Session Deleted", $"{sessionIds.Count} sessions were deleted", null);
        }
        return result;
    }

    public async Task<List<AdminFieldOption>> GetProcessUserOptionsAsync()
    {
        return await _adminRepository.GetProcessUserOptionsAsync();
    }

    public async Task<List<AdminFieldOption>> GetProcessStatusOptionsAsync()
    {
        return await _adminRepository.GetProcessStatusOptionsAsync();
    }

    public async Task<List<AdminFieldOption>> GetSessionUserOptionsAsync()
    {
        return await _adminRepository.GetSessionUserOptionsAsync();
    }

    public async Task<List<AdminFieldOption>> GetAlgorithmOptionsAsync()
    {
        return await _adminRepository.GetAlgorithmOptionsAsync();
    }

    public async Task<byte[]> ExportProcessesToExcelAsync(string? searchTerm, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var processes = await _adminRepository.GetProcessesPagedAsync(1, 10000, searchTerm, "CreatedAt", "DESC", userFilter, statusFilter, dateFrom, dateTo);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Processes");

        sheet.Cell(1, 1).Value = "ID";
        sheet.Cell(1, 2).Value = "Process Name";
        sheet.Cell(1, 3).Value = "Process PID";
        sheet.Cell(1, 4).Value = "Arrival Time";
        sheet.Cell(1, 5).Value = "Burst Time";
        sheet.Cell(1, 6).Value = "Deadline";
        sheet.Cell(1, 7).Value = "Priority";
        sheet.Cell(1, 8).Value = "Status";
        sheet.Cell(1, 9).Value = "Created By";
        sheet.Cell(1, 10).Value = "Created Date";

        var headerRow = sheet.Row(1);
        foreach (var cell in headerRow.Cells())
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var p in processes.Items)
        {
            sheet.Cell(row, 1).Value = p.Id;
            sheet.Cell(row, 2).Value = p.ProcessName;
            sheet.Cell(row, 3).Value = p.ProcessId;
            sheet.Cell(row, 4).Value = p.ArrivalTime;
            sheet.Cell(row, 5).Value = p.BurstTime;
            sheet.Cell(row, 6).Value = p.Deadline;
            sheet.Cell(row, 7).Value = p.Priority;
            sheet.Cell(row, 8).Value = p.Status;
            sheet.Cell(row, 9).Value = p.CreatedByName ?? "N/A";
            sheet.Cell(row, 10).Value = p.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportProcessesToPdfAsync(string? searchTerm, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var processes = await _adminRepository.GetProcessesPagedAsync(1, 10000, searchTerm, "CreatedAt", "DESC", userFilter, statusFilter, dateFrom, dateTo);

        using var stream = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate(), 15, 15, 15, 15);
        var writer = PdfWriter.GetInstance(document, stream);
        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

        document.Add(new Paragraph("ERDRR - Process Management Report", titleFont));
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}", normalFont));
        document.Add(new Paragraph(" "));

        var table = new PdfPTable(8);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 8, 12, 10, 10, 10, 10, 15, 15 });

        string[] headers = { "PID", "Name", "Arrival", "Burst", "Deadline", "Priority", "Status", "Created Date" };
        foreach (var h in headers)
        {
            var cell = new Phrase(h, headerFont);
            var pdfCell = new PdfPCell(cell);
            pdfCell.BackgroundColor = new BaseColor(33, 37, 41);
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Padding = 5;
            table.AddCell(pdfCell);
        }

        foreach (var p in processes.Items)
        {
            table.AddCell(new Phrase(p.ProcessId, normalFont));
            table.AddCell(new Phrase(p.ProcessName, normalFont));
            table.AddCell(new Phrase(p.ArrivalTime.ToString(), normalFont));
            table.AddCell(new Phrase(p.BurstTime.ToString(), normalFont));
            table.AddCell(new Phrase(p.Deadline.ToString(), normalFont));
            table.AddCell(new Phrase(p.Priority.ToString(), normalFont));
            table.AddCell(new Phrase(p.Status, normalFont));
            table.AddCell(new Phrase(p.CreatedAt.ToString("yyyy-MM-dd"), normalFont));
        }

        document.Add(table);
        document.Close();

        return stream.ToArray();
    }

    public async Task<byte[]> ExportSessionsToExcelAsync(string? searchTerm, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var sessions = await _adminRepository.GetSessionsPagedAsync(1, 10000, searchTerm, "CreatedAt", "DESC", algorithmFilter, userFilter, dateFrom, dateTo);

        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Sessions");

        sheet.Cell(1, 1).Value = "ID";
        sheet.Cell(1, 2).Value = "Session Name";
        sheet.Cell(1, 3).Value = "Algorithm";
        sheet.Cell(1, 4).Value = "Quantum";
        sheet.Cell(1, 5).Value = "Preemptive";
        sheet.Cell(1, 6).Value = "Process Count";
        sheet.Cell(1, 7).Value = "Status";
        sheet.Cell(1, 8).Value = "Created By";
        sheet.Cell(1, 9).Value = "Created Date";

        var headerRow = sheet.Row(1);
        foreach (var cell in headerRow.Cells())
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
            cell.Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var s in sessions.Items)
        {
            sheet.Cell(row, 1).Value = s.Id;
            sheet.Cell(row, 2).Value = s.SessionName;
            sheet.Cell(row, 3).Value = s.AlgorithmType;
            sheet.Cell(row, 4).Value = s.TimeQuantum;
            sheet.Cell(row, 5).Value = s.IsPreemptive ? "Yes" : "No";
            sheet.Cell(row, 6).Value = s.ProcessCount;
            sheet.Cell(row, 7).Value = s.Status;
            sheet.Cell(row, 8).Value = s.CreatedByName ?? "N/A";
            sheet.Cell(row, 9).Value = s.CreatedAt.ToString("yyyy-MM-dd HH:mm");
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportSessionsToPdfAsync(string? searchTerm, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var sessions = await _adminRepository.GetSessionsPagedAsync(1, 10000, searchTerm, "CreatedAt", "DESC", algorithmFilter, userFilter, dateFrom, dateTo);

        using var stream = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate(), 15, 15, 15, 15);
        var writer = PdfWriter.GetInstance(document, stream);
        document.Open();

        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

        document.Add(new Paragraph("ERDRR - Session Management Report", titleFont));
        document.Add(new Paragraph($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}", normalFont));
        document.Add(new Paragraph(" "));

        var table = new PdfPTable(7);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 15, 12, 10, 12, 12, 15, 14 });

        string[] headers = { "Session Name", "Algorithm", "Quantum", "Preemptive", "Processes", "Status", "Created Date" };
        foreach (var h in headers)
        {
            var cell = new Phrase(h, headerFont);
            var pdfCell = new PdfPCell(cell);
            pdfCell.BackgroundColor = new BaseColor(33, 37, 41);
            pdfCell.HorizontalAlignment = Element.ALIGN_CENTER;
            pdfCell.Padding = 5;
            table.AddCell(pdfCell);
        }

        foreach (var s in sessions.Items)
        {
            table.AddCell(new Phrase(s.SessionName, normalFont));
            table.AddCell(new Phrase(s.AlgorithmType, normalFont));
            table.AddCell(new Phrase(s.TimeQuantum.ToString(), normalFont));
            table.AddCell(new Phrase(s.IsPreemptive ? "Yes" : "No", normalFont));
            table.AddCell(new Phrase(s.ProcessCount.ToString(), normalFont));
            table.AddCell(new Phrase(s.Status, normalFont));
            table.AddCell(new Phrase(s.CreatedAt.ToString("yyyy-MM-dd"), normalFont));
        }

        document.Add(table);
        document.Close();

        return stream.ToArray();
    }
}
