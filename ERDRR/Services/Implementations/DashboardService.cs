using ERDRR.Models.DTOs;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Interfaces;

namespace ERDRR.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IProcessRepository _processRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IExecutionLogRepository _executionLogRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IProcessRepository processRepository,
        ISessionRepository sessionRepository,
        IResultRepository resultRepository,
        IExecutionLogRepository executionLogRepository,
        ILogger<DashboardService> logger)
    {
        _processRepository = processRepository;
        _sessionRepository = sessionRepository;
        _resultRepository = resultRepository;
        _executionLogRepository = executionLogRepository;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardDataAsync()
    {
        try
        {
            var totalSessions = await _sessionRepository.CountAsync(s => !s.IsDeleted);
            var totalProcesses = await _processRepository.CountAsync(p => !p.IsDeleted);
            var allSessions = (await _sessionRepository.GetAllAsync())
                .Where(s => !s.IsDeleted)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return new DashboardDto
            {
                TotalSessions = totalSessions,
                TotalProcesses = totalProcesses,
                AllSessions = allSessions.Select(s => new SessionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    AlgorithmType = s.AlgorithmType,
                    TimeQuantum = s.TimeQuantum,
                    Status = s.Status,
                    IsPreemptive = s.IsPreemptive,
                    CreatedAt = s.CreatedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            return new DashboardDto();
        }
    }

    public async Task<DashboardDto> GetDashboardDataForSessionAsync(int sessionId)
    {
        try
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null || session.IsDeleted)
            {
                _logger.LogWarning("Session {SessionId} not found or deleted", sessionId);
                return await GetDashboardDataAsync();
            }

            var selectedSessionResults = (await _resultRepository.GetBySessionIdAsync(sessionId)).ToList();
            var allSessionResults = (await _resultRepository.GetAllWithSessionInfoAsync()).ToList();
            var allSessions = (await _sessionRepository.GetAllAsync())
                .Where(s => !s.IsDeleted)
                .ToList();
            var totalProcesses = await _processRepository.CountAsync(p => !p.IsDeleted);
            var totalSessions = allSessions.Count;

            double avgWaitingTime = selectedSessionResults.Count > 0
                ? selectedSessionResults.Average(r => r.WaitingTime) : 0;
            double avgTurnaroundTime = selectedSessionResults.Count > 0
                ? selectedSessionResults.Average(r => r.TurnaroundTime) : 0;
            double avgResponseTime = selectedSessionResults.Count > 0
                ? selectedSessionResults.Average(r => r.ResponseTime) : 0;
            double avgCpuUtil = selectedSessionResults.Count > 0
                ? selectedSessionResults.Average(r => r.CpuUtilization) : 0;
            double avgThroughput = selectedSessionResults.Count > 0
                ? selectedSessionResults.Average(r => r.Throughput) : 0;
            int contextSwitches = selectedSessionResults.Count > 0
                ? selectedSessionResults.First().ContextSwitchCount : 0;
            int missedDeadlines = selectedSessionResults.Count(r => r.IsMissedDeadline);
            int completedProcesses = selectedSessionResults.Count;
            int totalTime = selectedSessionResults.Count > 0
                ? selectedSessionResults.Max(r => r.EndTime) : 0;
            double deadlineSuccess = completedProcesses > 0
                ? Math.Round((double)(completedProcesses - missedDeadlines) / completedProcesses * 100, 1)
                : 0;

            var processResults = selectedSessionResults.Select(r => new ProcessResultDto
            {
                ProcessId = r.ProcessId,
                ProcessName = r.ProcessName,
                ArrivalTime = r.ArrivalTime,
                BurstTime = r.BurstTime,
                Deadline = r.Deadline,
                CompletionTime = r.CompletionTime,
                WaitingTime = r.WaitingTime,
                TurnaroundTime = r.TurnaroundTime,
                ResponseTime = r.ResponseTime,
                MissedDeadline = r.IsMissedDeadline
            }).ToList();

            var completed = selectedSessionResults.Count(r => !r.IsMissedDeadline);
            var missed = selectedSessionResults.Count(r => r.IsMissedDeadline);
            var processStats = new List<ProcessStatisticsDto>
            {
                new() { Status = "Completed", Count = completed },
                new() { Status = "Missed Deadline", Count = missed }
            };

            var sessionPerformances = new List<SessionPerformanceDto>();
            foreach (var s in allSessions)
            {
                var sessionResults = allSessionResults
                    .Where(r => r.SchedulingSessionId == s.Id)
                    .ToList();

                if (sessionResults.Count > 0)
                {
                    sessionPerformances.Add(new SessionPerformanceDto
                    {
                        SessionName = s.Name,
                        AlgorithmType = s.AlgorithmType,
                        CpuUtilization = Math.Round(sessionResults.Average(r => r.CpuUtilization), 2),
                        Throughput = Math.Round(sessionResults.Average(r => r.Throughput), 4),
                        MissedDeadlines = sessionResults.Count(r => r.IsMissedDeadline)
                    });
                }
            }

            var allSessionsList = allSessions.Select(s => new SessionDto
            {
                Id = s.Id,
                Name = s.Name,
                AlgorithmType = s.AlgorithmType,
                TimeQuantum = s.TimeQuantum,
                Status = s.Status,
                IsPreemptive = s.IsPreemptive,
                CreatedAt = s.CreatedAt
            }).ToList();

            return new DashboardDto
            {
                TotalSessions = totalSessions,
                TotalProcesses = totalProcesses,

                SessionId = sessionId,
                SessionName = session.Name,
                AlgorithmType = session.AlgorithmType,
                SessionCreatedDate = session.CreatedAt,

                SessionProcessCount = selectedSessionResults.Count,
                CompletedProcesses = completedProcesses,
                MissedDeadlines = missedDeadlines,
                TotalExecutionTime = totalTime,
                AverageWaitingTime = Math.Round(avgWaitingTime, 2),
                AverageTurnaroundTime = Math.Round(avgTurnaroundTime, 2),
                AverageResponseTime = Math.Round(avgResponseTime, 2),
                CpuUtilization = Math.Round(avgCpuUtil, 2),
                Throughput = Math.Round(avgThroughput, 4),
                ContextSwitchCount = contextSwitches,
                DeadlineSuccessRate = deadlineSuccess,

                ProcessResults = processResults,
                ProcessStatistics = processStats,
                SessionPerformances = sessionPerformances,
                AllSessions = allSessionsList
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data for session {SessionId}", sessionId);
            return await GetDashboardDataAsync();
        }
    }

    public async Task<DashboardDto> GetDashboardDataForUserAsync(string userId)
    {
        try
        {
            var totalSessions = await _sessionRepository.CountByUserIdAsync(userId);
            var totalProcesses = await _processRepository.CountAsync(p => !p.IsDeleted);
            var userSessions = (await _sessionRepository.GetByUserIdAsync(userId)).ToList();

            return new DashboardDto
            {
                TotalSessions = totalSessions,
                TotalProcesses = totalProcesses,
                AllSessions = userSessions.Select(s => new SessionDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    AlgorithmType = s.AlgorithmType,
                    TimeQuantum = s.TimeQuantum,
                    Status = s.Status,
                    IsPreemptive = s.IsPreemptive,
                    CreatedAt = s.CreatedAt
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data for user {UserId}", userId);
            return new DashboardDto();
        }
    }
}
