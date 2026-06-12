using EDFRR.Models.Entities;

namespace EDFRR.Repositories.Interfaces;

public interface IResultRepository : IRepository<SchedulingResult>
{
    Task<IEnumerable<SchedulingResult>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<SchedulingResult>> GetAllWithSessionInfoAsync();
    Task<SchedulingResult?> GetLatestBySessionIdAsync(int sessionId);
    Task ClearResultsForSessionAsync(int sessionId);
}

public interface IExecutionLogRepository : IRepository<ExecutionLog>
{
    Task<IEnumerable<ExecutionLog>> GetBySessionIdAsync(int sessionId);
    Task<IEnumerable<ExecutionLog>> GetBySessionAndTimeStepAsync(int sessionId, int timeStep);
    Task ClearLogsForSessionAsync(int sessionId);
}
