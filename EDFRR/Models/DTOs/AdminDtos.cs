namespace EDFRR.Models.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
    public string ShowingText => TotalCount == 0
        ? "No records found"
        : $"Showing {((PageNumber - 1) * PageSize) + 1}-{Math.Min(PageNumber * PageSize, TotalCount)} of {TotalCount} records";
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int AdminCount { get; set; }
    public int UserCount { get; set; }
    public int TodayLogins { get; set; }
    public int TotalProcesses { get; set; }
    public int TotalSessions { get; set; }
    public int TotalSimulations { get; set; }
    public int TotalComparisons { get; set; }
    public int TotalReports { get; set; }
    public List<MonthlyRegistrationDto> RegistrationsPerMonth { get; set; } = new();
    public List<AlgorithmUsageDto> AlgorithmUsage { get; set; } = new();
    public List<DailyActivityDto> ProcessTrends { get; set; } = new();
    public List<DailyActivityDto> SessionTrends { get; set; } = new();
    public List<DailyActivityDto> SimulationTrends { get; set; } = new();
    public List<RecentUserDto> RecentUsers { get; set; } = new();
    public List<RecentSessionDto> RecentSessions { get; set; } = new();
    public List<TopUserByProcessDto> TopUsersByProcessCount { get; set; } = new();
}

public class MonthlyRegistrationDto
{
    public string MonthLabel { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AlgorithmUsageDto
{
    public string AlgorithmType { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyActivityDto
{
    public DateTime Day { get; set; }
    public int Count { get; set; }
}

public class AdminUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public string? PhoneNumber { get; set; }
    public string Roles { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AdminUserDetailDto : AdminUserDto
{
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public int TotalSessions { get; set; }
    public int TotalProcesses { get; set; }
    public int TotalSimulations { get; set; }
    public int TotalComparisons { get; set; }
    public List<AdminUserActivityDto> RecentActivities { get; set; } = new();
}

public class AdminUserActivityDto
{
    public string Activity { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class AdminAuditLogDto
{
    public string Action { get; set; } = string.Empty;
    public string TargetUserId { get; set; } = string.Empty;
    public string TargetUserName { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class RecentUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RecentSessionDto
{
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public int ProcessCount { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string CreatedByEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TopUserByProcessDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int ProcessCount { get; set; }
}
