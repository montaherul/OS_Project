using ERDRR.Data;
using ERDRR.Models.DTOs;
using ERDRR.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ERDRR.Repositories.Implementations;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _context;

    public AdminRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminUserDto>> GetUsersPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? roleFilter, string? statusFilter)
    {
        var users = new List<AdminUserDto>();
        int totalCount = 0;

        var connection = _context.Database.GetDbConnection();
        await using var _ = connection as SqlConnection ?? throw new InvalidOperationException("Expected SqlConnection");
        var conn = (SqlConnection)connection;
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_GetUsersPaged", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortColumn", sortColumn);
        cmd.Parameters.AddWithValue("@SortDirection", sortDirection);
        cmd.Parameters.AddWithValue("@RoleFilter", (object?)roleFilter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusFilter", (object?)statusFilter ?? DBNull.Value);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new AdminUserDto
            {
                UserId = reader.GetString(reader.GetOrdinal("UserId")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                LastLoginAt = reader.IsDBNull(reader.GetOrdinal("LastLoginAt"))
                    ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginAt")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                LockoutEnd = reader.IsDBNull(reader.GetOrdinal("LockoutEnd"))
                    ? (DateTimeOffset?)null : reader.GetDateTimeOffset(reader.GetOrdinal("LockoutEnd")),
                AccessFailedCount = reader.GetInt32(reader.GetOrdinal("AccessFailedCount")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                    ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Roles = reader.GetString(reader.GetOrdinal("Roles")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            });
        }

        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
        }

        return new PagedResult<AdminUserDto>
        {
            Items = users,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminUserDetailDto?> GetUserDetailsAsync(string userId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_GetUserDetails", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = await cmd.ExecuteReaderAsync();

        AdminUserDetailDto? detail = null;

        if (await reader.ReadAsync())
        {
            detail = new AdminUserDetailDto
            {
                UserId = reader.GetString(reader.GetOrdinal("UserId")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                LastLoginAt = reader.IsDBNull(reader.GetOrdinal("LastLoginAt"))
                    ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastLoginAt")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                LockoutEnd = reader.IsDBNull(reader.GetOrdinal("LockoutEnd"))
                    ? (DateTimeOffset?)null : reader.GetDateTimeOffset(reader.GetOrdinal("LockoutEnd")),
                AccessFailedCount = reader.GetInt32(reader.GetOrdinal("AccessFailedCount")),
                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber"))
                    ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                Roles = reader.GetString(reader.GetOrdinal("Roles")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                EmailConfirmed = reader.GetBoolean(reader.GetOrdinal("EmailConfirmed")),
                PhoneNumberConfirmed = reader.GetBoolean(reader.GetOrdinal("PhoneNumberConfirmed")),
                TotalSessions = reader.GetInt32(reader.GetOrdinal("TotalSessions")),
                TotalProcesses = reader.GetInt32(reader.GetOrdinal("TotalProcesses")),
                TotalSimulations = reader.GetInt32(reader.GetOrdinal("TotalSimulations")),
                TotalComparisons = reader.GetInt32(reader.GetOrdinal("TotalComparisons"))
            };
        }

        if (detail == null) return null;

        if (await reader.NextResultAsync())
        {
            var activities = new List<AdminUserActivityDto>();
            while (await reader.ReadAsync())
            {
                activities.Add(new AdminUserActivityDto
                {
                    Activity = reader.GetString(reader.GetOrdinal("Activity")),
                    Details = reader.GetString(reader.GetOrdinal("Details")),
                    Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                });
            }
            detail.RecentActivities = activities;
        }

        return detail;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync()
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_GetDashboardStats", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        using var reader = await cmd.ExecuteReaderAsync();

        var dto = new AdminDashboardDto();

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
                dto.SimulationTrends.Add(new DailyActivityDto
                {
                    Day = reader.GetDateTime(reader.GetOrdinal("Day")),
                    Count = reader.GetInt32(reader.GetOrdinal("Count"))
                });
            }
        }

        return dto;
    }

    public async Task<bool> UpdateUserAsync(string userId, string firstName, string lastName, string email, string? phoneNumber, bool isActive)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_UpdateUser", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@FirstName", firstName);
        cmd.Parameters.AddWithValue("@LastName", lastName);
        cmd.Parameters.AddWithValue("@Email", email);
        cmd.Parameters.AddWithValue("@PhoneNumber", (object?)phoneNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", isActive);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_DeleteUser", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> LockUserAsync(string userId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_LockUser", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> UnlockUserAsync(string userId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_UnlockUser", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserId", userId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> BulkDeleteUsersAsync(List<string> userIds)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_BulkDeleteUsers", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserIds", string.Join(",", userIds));

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> BulkLockUsersAsync(List<string> userIds)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_BulkLockUsers", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserIds", string.Join(",", userIds));

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> BulkUnlockUsersAsync(List<string> userIds)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_BulkUnlockUsers", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@UserIds", string.Join(",", userIds));

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<PagedResult<AdminProcessListDto>> GetProcessesPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? userFilter, string? statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var items = new List<AdminProcessListDto>();
        int totalCount = 0;

        var connection = _context.Database.GetDbConnection();
        await using var _ = connection as SqlConnection ?? throw new InvalidOperationException("Expected SqlConnection");
        var conn = (SqlConnection)connection;
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_GetProcessesPaged", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortColumn", sortColumn ?? "CreatedAt");
        cmd.Parameters.AddWithValue("@SortDirection", sortDirection ?? "DESC");
        cmd.Parameters.AddWithValue("@UserFilter", (object?)userFilter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusFilter", (object?)statusFilter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DateFrom", dateFrom.HasValue ? (object)dateFrom.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@DateTo", dateTo.HasValue ? (object)dateTo.Value : DBNull.Value);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AdminProcessListDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProcessName = reader.GetString(reader.GetOrdinal("ProcessName")),
                ProcessId = reader.GetString(reader.GetOrdinal("ProcessId")),
                ArrivalTime = reader.GetInt32(reader.GetOrdinal("ArrivalTime")),
                BurstTime = reader.GetInt32(reader.GetOrdinal("BurstTime")),
                Deadline = reader.GetInt32(reader.GetOrdinal("Deadline")),
                Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? null : reader.GetString(reader.GetOrdinal("CreatedByName")),
                CreatedByEmail = reader.IsDBNull(reader.GetOrdinal("CreatedByEmail")) ? null : reader.GetString(reader.GetOrdinal("CreatedByEmail")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                SchedulingSessionId = reader.GetInt32(reader.GetOrdinal("SchedulingSessionId"))
            });
        }

        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
        }

        return new PagedResult<AdminProcessListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminProcessDetailDto?> GetProcessDetailsAsync(int processId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_GetProcessDetails", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ProcessId", processId);

        using var reader = await cmd.ExecuteReaderAsync();
        AdminProcessDetailDto? detail = null;

        if (await reader.ReadAsync())
        {
            detail = new AdminProcessDetailDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ProcessName = reader.GetString(reader.GetOrdinal("ProcessName")),
                ProcessId = reader.GetString(reader.GetOrdinal("ProcessId")),
                ArrivalTime = reader.GetInt32(reader.GetOrdinal("ArrivalTime")),
                BurstTime = reader.GetInt32(reader.GetOrdinal("BurstTime")),
                Deadline = reader.GetInt32(reader.GetOrdinal("Deadline")),
                Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? null : reader.GetString(reader.GetOrdinal("CreatedByName")),
                CreatedByEmail = reader.IsDBNull(reader.GetOrdinal("CreatedByEmail")) ? null : reader.GetString(reader.GetOrdinal("CreatedByEmail")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                SchedulingSessionId = reader.GetInt32(reader.GetOrdinal("SchedulingSessionId")),
                SessionName = reader.IsDBNull(reader.GetOrdinal("SessionName")) ? "N/A" : reader.GetString(reader.GetOrdinal("SessionName")),
                AlgorithmType = reader.GetString(reader.GetOrdinal("AlgorithmType")),
                CompletionTime = reader.IsDBNull(reader.GetOrdinal("CompletionTime")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("CompletionTime")),
                TurnaroundTime = reader.IsDBNull(reader.GetOrdinal("TurnaroundTime")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("TurnaroundTime")),
                WaitingTime = reader.IsDBNull(reader.GetOrdinal("WaitingTime")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("WaitingTime")),
                ResponseTime = reader.IsDBNull(reader.GetOrdinal("ResponseTime")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ResponseTime"))
            };
        }

        return detail;
    }

    public async Task<bool> DeleteProcessAsync(int processId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_DeleteProcess", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ProcessId", processId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> BulkDeleteProcessesAsync(List<int> processIds)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_BulkDeleteProcesses", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@ProcessIds", string.Join(",", processIds));

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<PagedResult<AdminSessionListDto>> GetSessionsPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, string? sortColumn, string? sortDirection, string? algorithmFilter, string? userFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var items = new List<AdminSessionListDto>();
        int totalCount = 0;

        var connection = _context.Database.GetDbConnection();
        await using var _ = connection as SqlConnection ?? throw new InvalidOperationException("Expected SqlConnection");
        var conn = (SqlConnection)connection;
        await conn.OpenAsync();

        using var cmd = new SqlCommand("sp_GetSessionsPaged", conn) { CommandType = System.Data.CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);
        cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SortColumn", sortColumn ?? "CreatedAt");
        cmd.Parameters.AddWithValue("@SortDirection", sortDirection ?? "DESC");
        cmd.Parameters.AddWithValue("@AlgorithmFilter", (object?)algorithmFilter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@UserFilter", (object?)userFilter ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DateFrom", dateFrom.HasValue ? (object)dateFrom.Value : DBNull.Value);
        cmd.Parameters.AddWithValue("@DateTo", dateTo.HasValue ? (object)dateTo.Value : DBNull.Value);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            items.Add(new AdminSessionListDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SessionName = reader.GetString(reader.GetOrdinal("SessionName")),
                AlgorithmType = reader.GetString(reader.GetOrdinal("AlgorithmType")),
                TimeQuantum = reader.GetInt32(reader.GetOrdinal("TimeQuantum")),
                IsPreemptive = reader.GetBoolean(reader.GetOrdinal("IsPreemptive")),
                ProcessCount = reader.GetInt32(reader.GetOrdinal("ProcessCount")),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? null : reader.GetString(reader.GetOrdinal("CreatedByName")),
                CreatedByEmail = reader.IsDBNull(reader.GetOrdinal("CreatedByEmail")) ? null : reader.GetString(reader.GetOrdinal("CreatedByEmail")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                Status = reader.GetString(reader.GetOrdinal("Status"))
            });
        }

        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
        }

        return new PagedResult<AdminSessionListDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AdminSessionDetailDto?> GetSessionDetailsAsync(int sessionId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_GetSessionDetails", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await cmd.ExecuteReaderAsync();
        AdminSessionDetailDto? detail = null;

        if (await reader.ReadAsync())
        {
            detail = new AdminSessionDetailDto
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                SessionName = reader.GetString(reader.GetOrdinal("SessionName")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                AlgorithmType = reader.GetString(reader.GetOrdinal("AlgorithmType")),
                TimeQuantum = reader.GetInt32(reader.GetOrdinal("TimeQuantum")),
                IsPreemptive = reader.GetBoolean(reader.GetOrdinal("IsPreemptive")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedByName = reader.IsDBNull(reader.GetOrdinal("CreatedByName")) ? null : reader.GetString(reader.GetOrdinal("CreatedByName")),
                CreatedByEmail = reader.IsDBNull(reader.GetOrdinal("CreatedByEmail")) ? null : reader.GetString(reader.GetOrdinal("CreatedByEmail")),
                UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                CpuUtilization = reader.GetDouble(reader.GetOrdinal("CpuUtilization")),
                Throughput = reader.GetDouble(reader.GetOrdinal("Throughput")),
                AverageWaitingTime = reader.GetDouble(reader.GetOrdinal("AverageWaitingTime")),
                AverageTurnaroundTime = reader.GetDouble(reader.GetOrdinal("AverageTurnaroundTime")),
                AverageResponseTime = reader.GetDouble(reader.GetOrdinal("AverageResponseTime")),
                ContextSwitchCount = reader.GetInt32(reader.GetOrdinal("ContextSwitchCount")),
                DeadlineMissRatio = reader.GetDouble(reader.GetOrdinal("DeadlineMissRatio")),
                TotalProcesses = reader.GetInt32(reader.GetOrdinal("TotalProcesses")),
                CompletedProcesses = reader.GetInt32(reader.GetOrdinal("CompletedProcesses"))
            };
        }

        if (detail == null) return null;

        if (await reader.NextResultAsync())
        {
            var processes = new List<AdminSessionProcessDto>();
            while (await reader.ReadAsync())
            {
                processes.Add(new AdminSessionProcessDto
                {
                    ProcessId = reader.GetString(reader.GetOrdinal("ProcessId")),
                    ProcessName = reader.GetString(reader.GetOrdinal("ProcessName")),
                    ArrivalTime = reader.GetInt32(reader.GetOrdinal("ArrivalTime")),
                    BurstTime = reader.GetInt32(reader.GetOrdinal("BurstTime")),
                    Deadline = reader.GetInt32(reader.GetOrdinal("Deadline")),
                    Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
                    CompletionTime = reader.GetInt32(reader.GetOrdinal("CompletionTime")),
                    WaitingTime = reader.GetInt32(reader.GetOrdinal("WaitingTime")),
                    TurnaroundTime = reader.GetInt32(reader.GetOrdinal("TurnaroundTime")),
                    ResponseTime = reader.GetInt32(reader.GetOrdinal("ResponseTime")),
                    IsMissedDeadline = reader.GetBoolean(reader.GetOrdinal("IsMissedDeadline")),
                    StartTime = reader.GetInt32(reader.GetOrdinal("StartTime")),
                    EndTime = reader.GetInt32(reader.GetOrdinal("EndTime"))
                });
            }
            detail.Processes = processes;
        }

        if (await reader.NextResultAsync())
        {
            var gantt = new List<AdminGanttEntryDto>();
            while (await reader.ReadAsync())
            {
                gantt.Add(new AdminGanttEntryDto
                {
                    ProcessId = reader.GetString(reader.GetOrdinal("ProcessId")),
                    ProcessName = reader.GetString(reader.GetOrdinal("ProcessName")),
                    StartTime = reader.GetInt32(reader.GetOrdinal("StartTime")),
                    EndTime = reader.GetInt32(reader.GetOrdinal("EndTime")),
                    Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? string.Empty : reader.GetString(reader.GetOrdinal("Color")),
                    IsContextSwitch = reader.GetBoolean(reader.GetOrdinal("IsContextSwitch"))
                });
            }
            detail.GanttChart = gantt;
        }

        return detail;
    }

    public async Task<bool> DeleteSessionAsync(int sessionId)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_DeleteSession", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@SessionId", sessionId);

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> BulkDeleteSessionsAsync(List<int> sessionIds)
    {
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand("sp_BulkDeleteSessions", (SqlConnection)connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@SessionIds", string.Join(",", sessionIds));

        var rows = await cmd.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<List<AdminFieldOption>> GetProcessUserOptionsAsync()
    {
        var options = new List<AdminFieldOption>();
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT DISTINCT u.Id AS [Value], u.UserName AS [Label]
            FROM [Users] u INNER JOIN [Processes] p ON u.Id = p.UserId
            WHERE p.IsDeleted = 0
            ORDER BY u.UserName", (SqlConnection)connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            options.Add(new AdminFieldOption
            {
                Value = reader.GetString(reader.GetOrdinal("Value")),
                Label = reader.GetString(reader.GetOrdinal("Label"))
            });
        }
        return options;
    }

    public async Task<List<AdminFieldOption>> GetProcessStatusOptionsAsync()
    {
        var options = new List<AdminFieldOption>();
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT DISTINCT Status AS [Value], Status AS [Label]
            FROM [Processes] WHERE IsDeleted = 0
            ORDER BY Status", (SqlConnection)connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            options.Add(new AdminFieldOption
            {
                Value = reader.GetString(reader.GetOrdinal("Value")),
                Label = reader.GetString(reader.GetOrdinal("Label"))
            });
        }
        return options;
    }

    public async Task<List<AdminFieldOption>> GetSessionUserOptionsAsync()
    {
        var options = new List<AdminFieldOption>();
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT DISTINCT u.Id AS [Value], u.UserName AS [Label]
            FROM [Users] u INNER JOIN [SchedulingSessions] s ON u.Id = s.UserId
            WHERE s.IsDeleted = 0
            ORDER BY u.UserName", (SqlConnection)connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            options.Add(new AdminFieldOption
            {
                Value = reader.GetString(reader.GetOrdinal("Value")),
                Label = reader.GetString(reader.GetOrdinal("Label"))
            });
        }
        return options;
    }

    public async Task<List<AdminFieldOption>> GetAlgorithmOptionsAsync()
    {
        var options = new List<AdminFieldOption>();
        var connection = _context.Database.GetDbConnection();
        await connection.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT DISTINCT AlgorithmType AS [Value], AlgorithmType AS [Label]
            FROM [SchedulingSessions] WHERE IsDeleted = 0
            ORDER BY AlgorithmType", (SqlConnection)connection);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            options.Add(new AdminFieldOption
            {
                Value = reader.GetString(reader.GetOrdinal("Value")),
                Label = reader.GetString(reader.GetOrdinal("Label"))
            });
        }
        return options;
    }
}
