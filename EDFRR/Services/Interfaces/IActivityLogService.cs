using EDFRR.Models.DTOs;

namespace EDFRR.Services.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(string? userId, string action, string description, string? ipAddress);
    Task<List<ActivityLogDto>> GetRecentAsync(int count = 20);
}
