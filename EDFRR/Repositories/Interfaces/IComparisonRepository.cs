using EDFRR.Models.Entities;

namespace EDFRR.Repositories.Interfaces;

public interface IComparisonRepository : IRepository<AlgorithmComparison>
{
    Task<IEnumerable<AlgorithmComparison>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<AlgorithmComparison>> GetByUserIdAsync(string userId);
    Task<AlgorithmComparison?> GetLatestBySessionIdAsync(int sessionId);
    Task ClearForSessionAsync(int sessionId);
}
