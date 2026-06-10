using ERDRR.Models.Entities;

namespace ERDRR.Repositories.Interfaces;

public interface ISessionRepository : IRepository<SchedulingSession>
{
    Task<IEnumerable<SchedulingSession>> GetByUserIdAsync(string userId);
    Task<SchedulingSession?> GetWithProcessesAsync(int id);
    Task<SchedulingSession?> GetWithResultsAsync(int id);
    Task<SchedulingSession?> GetFullAsync(int id);
    Task<int> CountByUserIdAsync(string userId);
    Task<IEnumerable<SchedulingSession>> GetSessionsPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<int> CountFilteredAsync(string? searchTerm = null);
}
