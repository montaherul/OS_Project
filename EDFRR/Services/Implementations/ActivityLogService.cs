using EDFRR.Models.DTOs;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;

namespace EDFRR.Services.Implementations;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _repository;
    private readonly ILogger<ActivityLogService> _logger;

    public ActivityLogService(IActivityLogRepository repository, ILogger<ActivityLogService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task LogAsync(string? userId, string action, string description, string? ipAddress)
    {
        try
        {
            await _repository.LogAsync(userId, action, description, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log activity: {Action} by {UserId}", action, userId);
        }
    }

    public async Task<List<ActivityLogDto>> GetRecentAsync(int count = 20)
    {
        return await _repository.GetRecentAsync(count);
    }
}
