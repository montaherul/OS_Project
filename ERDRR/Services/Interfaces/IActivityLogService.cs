using ERDRR.Models.DTOs;

namespace ERDRR.Services.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(string? userId, string action, string description, string? ipAddress);
    Task<List<ActivityLogDto>> GetRecentAsync(int count = 20);
}
