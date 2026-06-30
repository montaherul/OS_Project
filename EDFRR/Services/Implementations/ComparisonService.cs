using EDFRR.Models.DTOs;
using EDFRR.Models.Entities;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;
using EDFRR.Scheduling.Engine;
using EDFRR.Scheduling.Models;

namespace EDFRR.Services.Implementations;

public class ComparisonService : IComparisonService
{
    private readonly IComparisonRepository _comparisonRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IProcessRepository _processRepository;
    private readonly SchedulingEngine _engine;
    private readonly ILogger<ComparisonService> _logger;

    public ComparisonService(
        IComparisonRepository comparisonRepository,
        ISessionRepository sessionRepository,
        IProcessRepository processRepository,
        SchedulingEngine engine,
        ILogger<ComparisonService> logger)
    {
        _comparisonRepository = comparisonRepository;
        _sessionRepository = sessionRepository;
        _processRepository = processRepository;
        _engine = engine;
        _logger = logger;
    }

    public async Task<ComparisonResultDto> CompareAlgorithmsAsync(int sessionId, string userId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null || session.IsDeleted)
            throw new InvalidOperationException($"Session {sessionId} not found.");

        var processes = (await _processRepository.GetBySessionIdAsync(sessionId)).ToList();
        if (processes.Count == 0)
            throw new InvalidOperationException("Session has no processes to compare.");

        var pcbList = processes.Select(p => new ProcessControlBlock
        {
            ProcessId = p.ProcessId,
            ProcessName = p.Name,
            ArrivalTime = p.ArrivalTime,
            BurstTime = p.BurstTime,
            RemainingTime = p.BurstTime,
            Deadline = p.Deadline,
            Priority = p.Priority,
            State = "New"
        }).ToList();

        var edfResult = _engine.RunSimulation(pcbList, "EDF", session.TimeQuantum, session.IsPreemptive);
        var rrResult = _engine.RunSimulation(pcbList, "RR", session.TimeQuantum, session.IsPreemptive);
        var hybridResult = _engine.RunSimulation(pcbList, "Hybrid", session.TimeQuantum, session.IsPreemptive);

        var edfMetrics = edfResult.Metrics;
        var rrMetrics = rrResult.Metrics;
        var hybridMetrics = hybridResult.Metrics;

        var (recommended, reason, score) = GetRecommendation(edfMetrics, rrMetrics, hybridMetrics);

        var comparison = new AlgorithmComparison
        {
            SchedulingSessionId = sessionId,
            UserId = userId,
            EDFWaitingTime = edfMetrics.AverageWaitingTime,
            EDFTurnaroundTime = edfMetrics.AverageTurnaroundTime,
            EDFResponseTime = edfMetrics.AverageResponseTime,
            EDFCPUUtilization = edfMetrics.CpuUtilization,
            EDFThroughput = edfMetrics.Throughput,
            EDFContextSwitches = edfMetrics.ContextSwitchCount,
            EDFDeadlineMissRatio = edfMetrics.DeadlineMissRatio,
            EDFExecutionTime = edfMetrics.TotalTime,
            RRWaitingTime = rrMetrics.AverageWaitingTime,
            RRTurnaroundTime = rrMetrics.AverageTurnaroundTime,
            RRResponseTime = rrMetrics.AverageResponseTime,
            RRCPUUtilization = rrMetrics.CpuUtilization,
            RRThroughput = rrMetrics.Throughput,
            RRContextSwitches = rrMetrics.ContextSwitchCount,
            RRDeadlineMissRatio = rrMetrics.DeadlineMissRatio,
            RRExecutionTime = rrMetrics.TotalTime,
            HybridWaitingTime = hybridMetrics.AverageWaitingTime,
            HybridTurnaroundTime = hybridMetrics.AverageTurnaroundTime,
            HybridResponseTime = hybridMetrics.AverageResponseTime,
            HybridCPUUtilization = hybridMetrics.CpuUtilization,
            HybridThroughput = hybridMetrics.Throughput,
            HybridContextSwitches = hybridMetrics.ContextSwitchCount,
            HybridDeadlineMissRatio = hybridMetrics.DeadlineMissRatio,
            HybridExecutionTime = hybridMetrics.TotalTime,
            RecommendedAlgorithm = recommended,
            RecommendationReason = reason,
            BestScore = score
        };

        await _comparisonRepository.AddAsync(comparison);

        return MapToDto(comparison, session, processes.Count);
    }

    public async Task<ComparisonResultDto?> GetComparisonAsync(int comparisonId)
    {
        var comparison = await _comparisonRepository.GetByIdAsync(comparisonId);
        if (comparison == null || comparison.IsDeleted) return null;

        var session = await _sessionRepository.GetByIdAsync(comparison.SchedulingSessionId);
        var processes = await _processRepository.GetBySessionIdAsync(comparison.SchedulingSessionId);

        return MapToDto(comparison, session, processes.Count());
    }

    public async Task<ComparisonResultDto?> GetLatestComparisonAsync(int sessionId)
    {
        var comparison = await _comparisonRepository.GetLatestBySessionIdAsync(sessionId);
        if (comparison == null) return null;

        var session = await _sessionRepository.GetByIdAsync(sessionId);
        var processes = await _processRepository.GetBySessionIdAsync(sessionId);

        return MapToDto(comparison, session, processes.Count());
    }

    public async Task<List<ComparisonResultDto>> GetUserComparisonsAsync(string userId)
    {
        var comparisons = await _comparisonRepository.GetByUserIdAsync(userId);
        var results = new List<ComparisonResultDto>();

        foreach (var c in comparisons)
        {
            var session = await _sessionRepository.GetByIdAsync(c.SchedulingSessionId);
            var processes = await _processRepository.GetBySessionIdAsync(c.SchedulingSessionId);
            results.Add(MapToDto(c, session, processes.Count()));
        }

        return results;
    }

    public async Task<ComparisonChartDataDto> GetChartDataAsync(int comparisonId)
    {
        var comparison = await _comparisonRepository.GetByIdAsync(comparisonId);
        if (comparison == null) return new ComparisonChartDataDto();

        return new ComparisonChartDataDto
        {
            Labels = new List<string> { "EDF", "RR", "Hybrid EDF+RR" },
            Datasets = new List<ChartDatasetDto>
            {
                new() { Label = "Waiting Time", Data = new List<double> { comparison.EDFWaitingTime, comparison.RRWaitingTime, comparison.HybridWaitingTime }, BackgroundColor = "rgba(255, 193, 7, 0.7)", BorderColor = "rgba(255, 193, 7, 1)" },
                new() { Label = "Turnaround Time", Data = new List<double> { comparison.EDFTurnaroundTime, comparison.RRTurnaroundTime, comparison.HybridTurnaroundTime }, BackgroundColor = "rgba(13, 110, 253, 0.7)", BorderColor = "rgba(13, 110, 253, 1)" },
                new() { Label = "Response Time", Data = new List<double> { comparison.EDFResponseTime, comparison.RRResponseTime, comparison.HybridResponseTime }, BackgroundColor = "rgba(23, 162, 184, 0.7)", BorderColor = "rgba(23, 162, 184, 1)" },
                new() { Label = "CPU Utilization %", Data = new List<double> { comparison.EDFCPUUtilization, comparison.RRCPUUtilization, comparison.HybridCPUUtilization }, BackgroundColor = "rgba(40, 167, 69, 0.7)", BorderColor = "rgba(40, 167, 69, 1)" },
                new() { Label = "Deadline Miss Ratio %", Data = new List<double> { comparison.EDFDeadlineMissRatio, comparison.RRDeadlineMissRatio, comparison.HybridDeadlineMissRatio }, BackgroundColor = "rgba(220, 53, 69, 0.7)", BorderColor = "rgba(220, 53, 69, 1)" }
            }
        };
    }

    public async Task<ComparisonExportDto> GetExportDataAsync(int comparisonId)
    {
        var comparison = await _comparisonRepository.GetByIdAsync(comparisonId);
        if (comparison == null) return new ComparisonExportDto();

        var session = await _sessionRepository.GetByIdAsync(comparison.SchedulingSessionId);
        var processes = await _processRepository.GetBySessionIdAsync(comparison.SchedulingSessionId);

        return new ComparisonExportDto
        {
            SessionId = comparison.SchedulingSessionId,
            SessionName = session?.Name ?? "",
            AlgorithmType = session?.AlgorithmType ?? "",
            ProcessCount = processes.Count(),
            GeneratedAt = comparison.CreatedAt,
            RecommendedAlgorithm = comparison.RecommendedAlgorithm,
            RecommendationReason = comparison.RecommendationReason,
            Metrics = new List<MetricComparisonRowDto>
            {
                CreateRow("Avg Waiting Time", comparison.EDFWaitingTime, comparison.RRWaitingTime, comparison.HybridWaitingTime, "lower"),
                CreateRow("Avg Turnaround Time", comparison.EDFTurnaroundTime, comparison.RRTurnaroundTime, comparison.HybridTurnaroundTime, "lower"),
                CreateRow("Avg Response Time", comparison.EDFResponseTime, comparison.RRResponseTime, comparison.HybridResponseTime, "lower"),
                CreateRow("CPU Utilization %", comparison.EDFCPUUtilization, comparison.RRCPUUtilization, comparison.HybridCPUUtilization, "higher"),
                CreateRow("Throughput", comparison.EDFThroughput, comparison.RRThroughput, comparison.HybridThroughput, "higher"),
                CreateRow("Context Switches", comparison.EDFContextSwitches, comparison.RRContextSwitches, comparison.HybridContextSwitches, "lower"),
                CreateRow("Deadline Miss Ratio %", comparison.EDFDeadlineMissRatio, comparison.RRDeadlineMissRatio, comparison.HybridDeadlineMissRatio, "lower")
            }
        };
    }

    public async Task DeleteComparisonAsync(int id)
    {
        var comparison = await _comparisonRepository.GetByIdAsync(id);
        if (comparison != null)
        {
            comparison.IsDeleted = true;
            comparison.UpdatedAt = DateTime.UtcNow;
            await _comparisonRepository.UpdateAsync(comparison);
        }
    }

    private static MetricComparisonRowDto CreateRow(string name, double edf, double rr, double hybrid, string preference)
    {
        double best = preference == "lower" ? Math.Min(edf, Math.Min(rr, hybrid)) : Math.Max(edf, Math.Max(rr, hybrid));
        string bestAlgo = preference == "lower"
            ? (best == edf ? "EDF" : best == rr ? "RR" : "Hybrid")
            : (best == edf ? "EDF" : best == rr ? "RR" : "Hybrid");

        return new MetricComparisonRowDto
        {
            MetricName = name,
            EDFValue = edf,
            RRValue = rr,
            HybridValue = hybrid,
            BestAlgorithm = bestAlgo
        };
    }

    private static (string recommended, string reason, double score) GetRecommendation(
        SchedulingMetrics edf, SchedulingMetrics rr, SchedulingMetrics hybrid)
    {
        var scores = new Dictionary<string, double>
        {
            ["EDF"] = CalculateScore(edf),
            ["RR"] = CalculateScore(rr),
            ["Hybrid"] = CalculateScore(hybrid)
        };

        var best = scores.OrderByDescending(kvp => kvp.Value).First();
        string reason = best.Key switch
        {
            "EDF" => $"EDF performs best for this workload because it has the lowest deadline miss ratio ({edf.DeadlineMissRatio}%) and optimizes for real-time constraints.",
            "RR" => $"RR performs best for this workload because it provides fair time-sharing with {rr.ContextSwitchCount} context switches and balanced waiting times.",
            "Hybrid" => $"Hybrid EDF+RR performs best for this workload because it combines deadline awareness with fair scheduling, achieving {hybrid.CpuUtilization}% CPU utilization and {hybrid.DeadlineMissRatio}% deadline miss ratio.",
            _ => "No clear winner."
        };

        return (best.Key, reason, best.Value);
    }

    private static double CalculateScore(SchedulingMetrics m)
    {
        double waitingScore = m.AverageWaitingTime > 0 ? 100.0 / (1.0 + m.AverageWaitingTime) : 100;
        double turnaroundScore = m.AverageTurnaroundTime > 0 ? 100.0 / (1.0 + m.AverageTurnaroundTime) : 100;
        double responseScore = m.AverageResponseTime > 0 ? 100.0 / (1.0 + m.AverageResponseTime) : 100;
        double cpuScore = m.CpuUtilization;
        double throughputScore = m.Throughput * 1000;
        double deadlineScore = 100.0 - m.DeadlineMissRatio;

        return Math.Round(
            waitingScore * 0.20 +
            turnaroundScore * 0.20 +
            responseScore * 0.15 +
            cpuScore * 0.20 +
            throughputScore * 0.10 +
            deadlineScore * 0.15, 2);
    }

    private static ComparisonResultDto MapToDto(AlgorithmComparison c, SchedulingSession? session, int processCount)
    {
        return new ComparisonResultDto
        {
            Id = c.Id,
            SessionId = c.SchedulingSessionId,
            SessionName = session?.Name ?? "",
            AlgorithmType = session?.AlgorithmType ?? "",
            ProcessCount = processCount,
            TimeQuantum = session?.TimeQuantum ?? 4,
            IsPreemptive = session?.IsPreemptive ?? true,
            CreatedAt = c.CreatedAt,
            EDF = new AlgorithmMetricsDto
            {
                WaitingTime = c.EDFWaitingTime,
                TurnaroundTime = c.EDFTurnaroundTime,
                ResponseTime = c.EDFResponseTime,
                CpuUtilization = c.EDFCPUUtilization,
                Throughput = c.EDFThroughput,
                ContextSwitches = c.EDFContextSwitches,
                DeadlineMissRatio = c.EDFDeadlineMissRatio,
                ExecutionTime = c.EDFExecutionTime
            },
            RR = new AlgorithmMetricsDto
            {
                WaitingTime = c.RRWaitingTime,
                TurnaroundTime = c.RRTurnaroundTime,
                ResponseTime = c.RRResponseTime,
                CpuUtilization = c.RRCPUUtilization,
                Throughput = c.RRThroughput,
                ContextSwitches = c.RRContextSwitches,
                DeadlineMissRatio = c.RRDeadlineMissRatio,
                ExecutionTime = c.RRExecutionTime
            },
            Hybrid = new AlgorithmMetricsDto
            {
                WaitingTime = c.HybridWaitingTime,
                TurnaroundTime = c.HybridTurnaroundTime,
                ResponseTime = c.HybridResponseTime,
                CpuUtilization = c.HybridCPUUtilization,
                Throughput = c.HybridThroughput,
                ContextSwitches = c.HybridContextSwitches,
                DeadlineMissRatio = c.HybridDeadlineMissRatio,
                ExecutionTime = c.HybridExecutionTime
            },
            RecommendedAlgorithm = c.RecommendedAlgorithm,
            RecommendationReason = c.RecommendationReason,
            BestScore = c.BestScore
        };
    }
}
