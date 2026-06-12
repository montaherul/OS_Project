using EDFRR.Data;
using EDFRR.Models.Entities;
using EDFRR.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EDFRR.Repositories.Implementations;

public class ResultRepository : Repository<SchedulingResult>, IResultRepository
{
    public ResultRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SchedulingResult>> GetBySessionIdAsync(int sessionId)
    {
        return await _dbSet
            .Where(r => r.SchedulingSessionId == sessionId && !r.IsDeleted)
            .OrderBy(r => r.ProcessId)
            .ToListAsync();
    }

    public async Task<IEnumerable<SchedulingResult>> GetAllWithSessionInfoAsync()
    {
        return await _dbSet
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.SchedulingSessionId)
            .ThenBy(r => r.ProcessId)
            .ToListAsync();
    }

    public async Task<SchedulingResult?> GetLatestBySessionIdAsync(int sessionId)
    {
        return await _dbSet
            .Where(r => r.SchedulingSessionId == sessionId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task ClearResultsForSessionAsync(int sessionId)
    {
        var results = await _dbSet
            .Where(r => r.SchedulingSessionId == sessionId)
            .ToListAsync();

        _dbSet.RemoveRange(results);
        await _context.SaveChangesAsync();
    }
}

public class ExecutionLogRepository : Repository<ExecutionLog>, IExecutionLogRepository
{
    public ExecutionLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExecutionLog>> GetBySessionIdAsync(int sessionId)
    {
        return await _dbSet
            .Where(l => l.SchedulingSessionId == sessionId && !l.IsDeleted)
            .OrderBy(l => l.TimeStep)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExecutionLog>> GetBySessionAndTimeStepAsync(int sessionId, int timeStep)
    {
        return await _dbSet
            .Where(l => l.SchedulingSessionId == sessionId && l.TimeStep == timeStep && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task ClearLogsForSessionAsync(int sessionId)
    {
        var logs = await _dbSet
            .Where(l => l.SchedulingSessionId == sessionId)
            .ToListAsync();

        _dbSet.RemoveRange(logs);
        await _context.SaveChangesAsync();
    }
}
