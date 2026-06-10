using ERDRR.Models.DTOs;

namespace ERDRR.Services.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync();
    Task<PagedResult<AdminUserDto>> GetUsersPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? roleFilter, string? statusFilter);
    Task<AdminUserDetailDto?> GetUserDetailsAsync(string userId);
    Task<bool> UpdateUserAsync(string userId, string firstName, string lastName, string email, string? phoneNumber, bool isActive);
    Task<bool> DeleteUserAsync(string userId, string performedBy);
    Task<bool> LockUserAsync(string userId, string performedBy);
    Task<bool> UnlockUserAsync(string userId, string performedBy);
    Task<bool> ResetPasswordAsync(string userId, string newPassword, string performedBy);
    Task<bool> AssignRoleAsync(string userId, string role);
    Task<bool> RemoveRoleAsync(string userId, string role);
    Task<bool> BulkDeleteUsersAsync(List<string> userIds);
    Task<bool> BulkLockUsersAsync(List<string> userIds);
    Task<bool> BulkUnlockUsersAsync(List<string> userIds);
    Task<byte[]> ExportUsersToExcelAsync(string? searchTerm, string? roleFilter, string? statusFilter);
    Task<byte[]> ExportUsersToPdfAsync(string? searchTerm, string? roleFilter, string? statusFilter);
    Task<PagedResult<AdminProcessListDto>> GetProcessesPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<AdminProcessDetailDto?> GetProcessDetailsAsync(int processId);
    Task<bool> DeleteProcessAsync(int processId, string performedBy);
    Task<bool> BulkDeleteProcessesAsync(List<int> processIds, string performedBy);
    Task<PagedResult<AdminSessionListDto>> GetSessionsPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<AdminSessionDetailDto?> GetSessionDetailsAsync(int sessionId);
    Task<bool> DeleteSessionAsync(int sessionId, string performedBy);
    Task<bool> BulkDeleteSessionsAsync(List<int> sessionIds, string performedBy);
    Task<List<AdminFieldOption>> GetProcessUserOptionsAsync();
    Task<List<AdminFieldOption>> GetProcessStatusOptionsAsync();
    Task<List<AdminFieldOption>> GetSessionUserOptionsAsync();
    Task<List<AdminFieldOption>> GetAlgorithmOptionsAsync();
    Task<byte[]> ExportProcessesToExcelAsync(string? searchTerm, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<byte[]> ExportProcessesToPdfAsync(string? searchTerm, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<byte[]> ExportSessionsToExcelAsync(string? searchTerm, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<byte[]> ExportSessionsToPdfAsync(string? searchTerm, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo);
}
