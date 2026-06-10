using ERDRR.Models.DTOs;

namespace ERDRR.Repositories.Interfaces;

public interface IActivityLogRepository
{
    Task LogAsync(string? userId, string action, string description, string? ipAddress);
    Task<List<ActivityLogDto>> GetRecentAsync(int count = 20);
}
