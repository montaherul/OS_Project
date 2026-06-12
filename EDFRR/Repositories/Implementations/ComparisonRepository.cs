using EDFRR.Data;
using EDFRR.Models.Entities;
using EDFRR.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EDFRR.Repositories.Implementations;

public class ComparisonRepository : Repository<AlgorithmComparison>, IComparisonRepository
{
    public ComparisonRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<AlgorithmComparison>> GetBySessionIdAsync(int sessionId)
    {
        return await _dbSet
            .Where(c => c.SchedulingSessionId == sessionId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AlgorithmComparison>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<AlgorithmComparison?> GetLatestBySessionIdAsync(int sessionId)
    {
        return await _dbSet
            .Where(c => c.SchedulingSessionId == sessionId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task ClearForSessionAsync(int sessionId)
    {
        var items = await _dbSet
            .Where(c => c.SchedulingSessionId == sessionId)
            .ToListAsync();

        _dbSet.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
