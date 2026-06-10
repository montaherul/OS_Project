using ERDRR.Models.DTOs;

namespace ERDRR.Services.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync();
    Task<List<RecentUserDto>> GetRecentUsersAsync(int count = 10);
    Task<List<RecentSessionDto>> GetRecentSessionsAsync(int count = 10);
    Task<List<AlgorithmUsageDto>> GetAlgorithmUsageAsync();
    Task<List<DailyActivityDto>> GetProcessTrendAsync();
    Task<List<DailyActivityDto>> GetSessionTrendAsync();
    Task<List<DailyActivityDto>> GetSimulationTrendAsync();
    Task<List<TopUserByProcessDto>> GetTopUsersByProcessCountAsync(int count = 10);
    Task<AdminDashboardDto> GetFullDashboardAsync();
}
