namespace EDFRR.Models.DTOs;

public class ProcessDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProcessId { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = "Pending";
    public int SchedulingSessionId { get; set; }
    public int CompletionTime { get; set; }
    public int TurnaroundTime { get; set; }
    public int WaitingTime { get; set; }
    public int ResponseTime { get; set; }
    public bool MissedDeadline { get; set; }
}

public class CreateProcessDto
{
    public string Name { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int SchedulingSessionId { get; set; }
}

public class SessionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AlgorithmType { get; set; } = "Hybrid";
    public int TimeQuantum { get; set; } = 4;
    public string Status { get; set; } = "Created";
    public bool IsPreemptive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public int ProcessCount { get; set; }
}

public class CreateSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AlgorithmType { get; set; } = "Hybrid";
    public int TimeQuantum { get; set; } = 4;
    public bool IsPreemptive { get; set; } = true;
}

public class GanttChartDto
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsContextSwitch { get; set; }
}

public class MetricsDto
{
    public double AverageWaitingTime { get; set; }
    public double AverageTurnaroundTime { get; set; }
    public double AverageResponseTime { get; set; }
    public double CpuUtilization { get; set; }
    public double Throughput { get; set; }
    public int ContextSwitchCount { get; set; }
    public int MissedDeadlines { get; set; }
    public double DeadlineMissRatio { get; set; }
    public int TotalProcesses { get; set; }
    public int CompletedProcesses { get; set; }
}

public class SimulationStepDto
{
    public int TimeStep { get; set; }
    public string? ExecutingProcess { get; set; }
    public List<string> ReadyQueue { get; set; } = new();
    public List<GanttChartDto> GanttEntries { get; set; } = new();
    public MetricsDto CurrentMetrics { get; set; } = new();
    public bool IsComplete { get; set; }
    public List<ProcessStateDto> ProcessStates { get; set; } = new();
}

public class ProcessStateDto
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string State { get; set; } = "New";
    public int RemainingTime { get; set; }
    public int? Deadline { get; set; }
    public bool MissedDeadline { get; set; }
}

public class DashboardDto
{
    // Global stats
    public int TotalSessions { get; set; }
    public int TotalProcesses { get; set; }

    // Session-specific stats (from SchedulingResult)
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public DateTime SessionCreatedDate { get; set; }
    public int SessionProcessCount { get; set; }
    public int CompletedProcesses { get; set; }
    public int MissedDeadlines { get; set; }
    public int TotalExecutionTime { get; set; }

    // Metrics (from SchedulingResult â€” session-scoped)
    public double AverageWaitingTime { get; set; }
    public double AverageTurnaroundTime { get; set; }
    public double AverageResponseTime { get; set; }
    public double CpuUtilization { get; set; }
    public double Throughput { get; set; }
    public int ContextSwitchCount { get; set; }
    public double DeadlineSuccessRate { get; set; }

    // Per-process results (from SchedulingResult â€” session-scoped)
    public List<ProcessResultDto> ProcessResults { get; set; } = new();

    // Session-scoped chart data
    public List<ProcessStatisticsDto> ProcessStatistics { get; set; } = new();

    // Cross-session comparison data (from ALL sessions' results)
    public List<SessionPerformanceDto> SessionPerformances { get; set; } = new();

    // All sessions for selector
    public List<SessionDto> AllSessions { get; set; } = new();

    // Gantt chart data
    public List<GanttChartDto> GanttChart { get; set; } = new();
}

public class ProcessResultDto
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int CompletionTime { get; set; }
    public int WaitingTime { get; set; }
    public int TurnaroundTime { get; set; }
    public int ResponseTime { get; set; }
    public bool MissedDeadline { get; set; }
}

public class ProcessStatisticsDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class SessionPerformanceDto
{
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public double CpuUtilization { get; set; }
    public double Throughput { get; set; }
    public int MissedDeadlines { get; set; }
}

public class ReportDto
{
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public MetricsDto Metrics { get; set; } = new();
    public List<ProcessDto> Processes { get; set; } = new();
    public List<GanttChartDto> GanttChart { get; set; } = new();
    public List<ExecutionLogDto> ExecutionLogs { get; set; } = new();
}

public class ExecutionLogDto
{
    public int TimeStep { get; set; }
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? ReadyQueue { get; set; }
}
