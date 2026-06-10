using ERDRR.Data;
using ERDRR.Models.DTOs;
using ERDRR.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace ERDRR.Services.Implementations;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AdminDashboardService> _logger;

    public AdminDashboardService(ApplicationDbContext context, ILogger<AdminDashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetFullDashboardAsync()
    {
        try
        {
            var result = await GetDashboardStatsAsync();
            result.RecentUsers = await GetRecentUsersAsync(10);
            result.RecentSessions = await GetRecentSessionsAsync(10);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading full admin dashboard");
            return new AdminDashboardDto();
        }
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync()
    {
        var dto = new AdminDashboardDto();

        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand("sp_GetDashboardStats", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                dto.TotalUsers = reader.GetInt32(reader.GetOrdinal("TotalUsers"));
                dto.ActiveUsers = reader.GetInt32(reader.GetOrdinal("ActiveUsers"));
                dto.AdminCount = reader.GetInt32(reader.GetOrdinal("AdminCount"));
                dto.UserCount = reader.GetInt32(reader.GetOrdinal("UserCount"));
                dto.TodayLogins = reader.GetInt32(reader.GetOrdinal("TodayLogins"));
                dto.TotalProcesses = reader.GetInt32(reader.GetOrdinal("TotalProcesses"));
                dto.TotalSessions = reader.GetInt32(reader.GetOrdinal("TotalSessions"));
                dto.TotalSimulations = reader.GetInt32(reader.GetOrdinal("TotalSimulations"));
                dto.TotalComparisons = reader.GetInt32(reader.GetOrdinal("TotalComparisons"));
                dto.TotalReports = reader.GetInt32(reader.GetOrdinal("TotalReports"));
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    dto.RegistrationsPerMonth.Add(new MonthlyRegistrationDto
                    {
                        MonthLabel = reader.GetString(reader.GetOrdinal("MonthLabel")),
                        Count = reader.GetInt32(reader.GetOrdinal("Count"))
                    });
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    dto.AlgorithmUsage.Add(new AlgorithmUsageDto
                    {
                        AlgorithmType = reader.GetString(reader.GetOrdinal("AlgorithmType")),
                        Count = reader.GetInt32(reader.GetOrdinal("Count"))
                    });
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    dto.ProcessTrends.Add(new DailyActivityDto
                    {
                        Day = reader.GetDateTime(reader.GetOrdinal("Day")),
                        Count = reader.GetInt32(reader.GetOrdinal("Count"))
                    });
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    dto.SessionTrends.Add(new DailyActivityDto
                    {
                        Day = reader.GetDateTime(reader.GetOrdinal("Day")),
                        Count = reader.GetInt32(reader.GetOrdinal("Count"))
                    });
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    dto.SimulationTrends.Add(new DailyActivityDto
                    {
                        Day = reader.GetDateTime(reader.GetOrdinal("Day")),
                        Count = reader.GetInt32(reader.GetOrdinal("Count"))
                    });
                }
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    dto.TopUsersByProcessCount.Add(new TopUserByProcessDto
                    {
                        UserId = reader.GetString(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                        LastName = reader.GetString(reader.GetOrdinal("LastName")),
                        ProcessCount = reader.GetInt32(reader.GetOrdinal("ProcessCount"))
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling sp_GetDashboardStats");
        }

        return dto;
    }

    public async Task<List<RecentUserDto>> GetRecentUsersAsync(int count = 10)
    {
        var users = new List<RecentUserDto>();

        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand("sp_GetRecentUsers", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Count", count);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new RecentUserDto
                {
                    UserId = reader.GetString(reader.GetOrdinal("UserId")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Role = reader.GetString(reader.GetOrdinal("Role")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling sp_GetRecentUsers");
        }

        return users;
    }

    public async Task<List<RecentSessionDto>> GetRecentSessionsAsync(int count = 10)
    {
        var sessions = new List<RecentSessionDto>();

        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand("sp_GetRecentSessions", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@Count", count);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                sessions.Add(new RecentSessionDto
                {
                    SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                    SessionName = reader.GetString(reader.GetOrdinal("SessionName")),
                    AlgorithmType = reader.GetString(reader.GetOrdinal("AlgorithmType")),
                    ProcessCount = reader.GetInt32(reader.GetOrdinal("ProcessCount")),
                    CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
                    CreatedByEmail = reader.GetString(reader.GetOrdinal("CreatedByEmail")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling sp_GetRecentSessions");
        }

        return sessions;
    }

    public async Task<List<AlgorithmUsageDto>> GetAlgorithmUsageAsync()
    {
        var usage = new List<AlgorithmUsageDto>();

        try
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();
            using var cmd = new SqlCommand("sp_GetAlgorithmUsage", (SqlConnection)connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                usage.Add(new AlgorithmUsageDto
                {
                    AlgorithmType = reader.GetString(reader.GetOrdinal("AlgorithmType")),
                    Count = reader.GetInt32(reader.GetOrdinal("Count"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling sp_GetAlgorithmUsage");
        }

        return usage;
    }

    public async Task<List<DailyActivityDto>> GetProcessTrendAsync()
    {
        try
        {
            var stats = await GetDashboardStatsAsync();
            return stats.ProcessTrends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading process trend");
            return new List<DailyActivityDto>();
        }
    }

    public async Task<List<DailyActivityDto>> GetSessionTrendAsync()
    {
        try
        {
            var stats = await GetDashboardStatsAsync();
            return stats.SessionTrends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading session trend");
            return new List<DailyActivityDto>();
        }
    }

    public async Task<List<DailyActivityDto>> GetSimulationTrendAsync()
    {
        try
        {
            var stats = await GetDashboardStatsAsync();
            return stats.SimulationTrends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading simulation trend");
            return new List<DailyActivityDto>();
        }
    }

    public async Task<List<TopUserByProcessDto>> GetTopUsersByProcessCountAsync(int count = 10)
    {
        try
        {
            var stats = await GetDashboardStatsAsync();
            return stats.TopUsersByProcessCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading top users by process");
            return new List<TopUserByProcessDto>();
        }
    }
}
