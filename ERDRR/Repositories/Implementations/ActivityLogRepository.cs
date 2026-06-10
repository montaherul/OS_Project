using ERDRR.Data;
using ERDRR.Models.DTOs;
using ERDRR.Models.Entities;
using ERDRR.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ERDRR.Repositories.Implementations;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string? userId, string action, string description, string? ipAddress)
    {
        var log = new ActivityLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            IPAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<ActivityLog>().Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ActivityLogDto>> GetRecentAsync(int count = 20)
    {
        var logs = new List<ActivityLogDto>();

        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_GetRecentActivities", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@Count", count);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            logs.Add(new ActivityLogDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
                Action = reader.GetString(reader.GetOrdinal("Action")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                IPAddress = reader.IsDBNull(reader.GetOrdinal("IPAddress")) ? null : reader.GetString(reader.GetOrdinal("IPAddress")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName"))
            });
        }

        return logs;
    }
}
