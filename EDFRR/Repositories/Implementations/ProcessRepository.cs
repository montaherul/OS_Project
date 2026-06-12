using EDFRR.Data;
using EDFRR.Models.Entities;
using EDFRR.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EDFRR.Repositories.Implementations;

public class ProcessRepository : Repository<ProcessEntity>, IProcessRepository
{
    public ProcessRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProcessEntity>> GetBySessionIdAsync(int sessionId)
    {
        return await _dbSet
            .Where(p => p.SchedulingSessionId == sessionId && !p.IsDeleted)
            .OrderBy(p => p.ArrivalTime)
            .ThenBy(p => p.ProcessId)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProcessEntity>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<ProcessEntity?> GetBySessionAndProcessIdAsync(int sessionId, string processId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SchedulingSessionId == sessionId && p.ProcessId == processId && !p.IsDeleted);
    }

    public async Task<ProcessEntity?> GetByNameAsync(int sessionId, string processName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.SchedulingSessionId == sessionId && p.Name == processName && !p.IsDeleted);
    }

    public async Task<int> CountBySessionIdAsync(int sessionId)
    {
        return await _dbSet.CountAsync(p => p.SchedulingSessionId == sessionId && !p.IsDeleted);
    }

    public async Task<int> CountAllBySessionIdAsync(int sessionId)
    {
        return await _dbSet.CountAsync(p => p.SchedulingSessionId == sessionId);
    }

    public async Task<int> GetMaxProcessIdNumberAsync(int sessionId)
    {
        var maxProcessId = await _dbSet
            .Where(p => p.SchedulingSessionId == sessionId && !p.IsDeleted)
            .Select(p => p.ProcessId)
            .OrderByDescending(p => p)
            .FirstOrDefaultAsync();

        if (string.IsNullOrEmpty(maxProcessId) || maxProcessId.Length < 2)
            return 0;

        if (int.TryParse(maxProcessId.Substring(1), out int number))
            return number;

        return 0;
    }

    public async Task<int> CountByStatusAsync(string status)
    {
        return await _dbSet.CountAsync(p => p.Status == status && !p.IsDeleted);
    }

    public async Task<IEnumerable<ProcessEntity>> GetProcessesPagedAsync(int sessionId, int page, int pageSize, string? searchTerm = null, string? status = null)
    {
        var query = _dbSet
            .Where(p => p.SchedulingSessionId == sessionId && !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.ProcessId.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        return await query
            .OrderBy(p => p.ArrivalTime)
            .ThenBy(p => p.ProcessId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(int sessionId, string? searchTerm = null, string? status = null)
    {
        var query = _dbSet
            .Where(p => p.SchedulingSessionId == sessionId && !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.ProcessId.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(p => p.Status == status);
        }

        return await query.CountAsync();
    }
}
