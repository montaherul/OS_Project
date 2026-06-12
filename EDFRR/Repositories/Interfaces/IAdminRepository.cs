using EDFRR.Models.DTOs;

namespace EDFRR.Repositories.Interfaces;

public interface IAdminRepository
{
    Task<PagedResult<AdminUserDto>> GetUsersPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? roleFilter, string? statusFilter);
    Task<AdminUserDetailDto?> GetUserDetailsAsync(string userId);
    Task<AdminDashboardDto> GetDashboardStatsAsync();
    Task<bool> UpdateUserAsync(string userId, string firstName, string lastName, string email, string? phoneNumber, bool isActive);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> LockUserAsync(string userId);
    Task<bool> UnlockUserAsync(string userId);
    Task<bool> BulkDeleteUsersAsync(List<string> userIds);
    Task<bool> BulkLockUsersAsync(List<string> userIds);
    Task<bool> BulkUnlockUsersAsync(List<string> userIds);
    Task<PagedResult<AdminProcessListDto>> GetProcessesPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<AdminProcessDetailDto?> GetProcessDetailsAsync(int processId);
    Task<bool> DeleteProcessAsync(int processId);
    Task<bool> BulkDeleteProcessesAsync(List<int> processIds);
    Task<PagedResult<AdminSessionListDto>> GetSessionsPagedAsync(int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo);
    Task<AdminSessionDetailDto?> GetSessionDetailsAsync(int sessionId);
    Task<bool> DeleteSessionAsync(int sessionId);
    Task<bool> BulkDeleteSessionsAsync(List<int> sessionIds);
    Task<List<AdminFieldOption>> GetProcessUserOptionsAsync();
    Task<List<AdminFieldOption>> GetProcessStatusOptionsAsync();
    Task<List<AdminFieldOption>> GetSessionUserOptionsAsync();
    Task<List<AdminFieldOption>> GetAlgorithmOptionsAsync();
}
