using ERDRR.Models.Entities;

namespace ERDRR.Repositories.Interfaces;

public interface IProcessRepository : IRepository<ProcessEntity>
{
    Task<IEnumerable<ProcessEntity>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<ProcessEntity>> GetByUserIdAsync(string userId);
    Task<ProcessEntity?> GetBySessionAndProcessIdAsync(int sessionId, string processId);
    Task<ProcessEntity?> GetByNameAsync(int sessionId, string processName);
    Task<int> CountBySessionIdAsync(int sessionId);
    Task<int> CountAllBySessionIdAsync(int sessionId);
    Task<int> CountByStatusAsync(string status);
    Task<IEnumerable<ProcessEntity>> GetProcessesPagedAsync(int sessionId, int page, int pageSize, string? searchTerm = null, string? status = null);
    Task<int> CountFilteredAsync(int sessionId, string? searchTerm = null, string? status = null);
    Task<int> GetMaxProcessIdNumberAsync(int sessionId);
}
