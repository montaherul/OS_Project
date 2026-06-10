using ERDRR.Data;
using ERDRR.Models.Entities;
using ERDRR.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERDRR.Repositories.Implementations;

public class SessionRepository : Repository<SchedulingSession>, ISessionRepository
{
    public SessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SchedulingSession>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<SchedulingSession?> GetWithProcessesAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Processes.Where(p => !p.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<SchedulingSession?> GetWithResultsAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Results)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<SchedulingSession?> GetFullAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Processes.Where(p => !p.IsDeleted))
            .Include(s => s.Results)
            .Include(s => s.ExecutionLogs)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        return await _dbSet.CountAsync(s => s.UserId == userId && !s.IsDeleted);
    }

    public async Task<IEnumerable<SchedulingSession>> GetSessionsPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var query = _dbSet.Where(s => !s.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => s.Name.Contains(searchTerm) || (s.Description != null && s.Description.Contains(searchTerm)));
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountFilteredAsync(string? searchTerm = null)
    {
        var query = _dbSet.Where(s => !s.IsDeleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s => s.Name.Contains(searchTerm) || (s.Description != null && s.Description.Contains(searchTerm)));
        }

        return await query.CountAsync();
    }
}
