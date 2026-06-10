using ERDRR.Models.DTOs;

namespace ERDRR.Models.ViewModels;

public class DashboardViewModel
{
    public DashboardDto Statistics { get; set; } = new();
    public List<SessionDto> RecentSessions { get; set; } = new();
    public int? SelectedSessionId { get; set; }
    public bool IsAdmin { get; set; }
}

public class ProcessListViewModel
{
    public List<ProcessDto> Processes { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string? SearchTerm { get; set; }
    public string? FilterStatus { get; set; }
    public int PageSize { get; set; } = 10;
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
}

public class ProcessCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int SchedulingSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
}

public class ProcessEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProcessId { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int SchedulingSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
}

public class SessionListViewModel
{
    public List<SessionDto> Sessions { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public string? SearchTerm { get; set; }
    public int PageSize { get; set; } = 10;
}

public class SessionCreateViewModel
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AlgorithmType { get; set; } = "Hybrid";
    public int TimeQuantum { get; set; } = 4;
    public bool IsPreemptive { get; set; } = true;
}

public class SessionEditViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AlgorithmType { get; set; } = "Hybrid";
    public int TimeQuantum { get; set; } = 4;
    public bool IsPreemptive { get; set; } = true;
    public string Status { get; set; } = "Created";
}

public class SimulationViewModel
{
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public int TimeQuantum { get; set; }
    public bool IsPreemptive { get; set; }
    public List<ProcessDto> Processes { get; set; } = new();
    public SimulationStepDto? CurrentStep { get; set; }
    public MetricsDto? FinalMetrics { get; set; }
    public List<GanttChartDto> GanttChart { get; set; } = new();
    public List<ExecutionLogDto> ExecutionLogs { get; set; } = new();
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }
    public bool IsComplete { get; set; }
}

public class ResultsViewModel
{
    public int SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public MetricsDto Metrics { get; set; } = new();
    public List<ProcessDto> Processes { get; set; } = new();
    public List<GanttChartDto> GanttChart { get; set; } = new();
    public List<ExecutionLogDto> ExecutionLogs { get; set; } = new();
}

public class ReportViewModel
{
    public List<SessionDto> Sessions { get; set; } = new();
    public int? SelectedSessionId { get; set; }
    public ReportDto? Report { get; set; }
}
