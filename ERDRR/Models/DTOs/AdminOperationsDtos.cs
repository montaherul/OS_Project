namespace ERDRR.Models.DTOs;

public class AdminProcessListDto
{
    public int Id { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ProcessId { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? CreatedByEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public int SchedulingSessionId { get; set; }
}

public class AdminProcessDetailDto
{
    public int Id { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ProcessId { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? CreatedByEmail { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int SchedulingSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public int? CompletionTime { get; set; }
    public int? TurnaroundTime { get; set; }
    public int? WaitingTime { get; set; }
    public int? ResponseTime { get; set; }
}

public class AdminSessionListDto
{
    public int Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string AlgorithmType { get; set; } = string.Empty;
    public int TimeQuantum { get; set; }
    public bool IsPreemptive { get; set; }
    public int ProcessCount { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedByEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AdminSessionDetailDto
{
    public int Id { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AlgorithmType { get; set; } = string.Empty;
    public int TimeQuantum { get; set; }
    public bool IsPreemptive { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? CreatedByEmail { get; set; }
    public string? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public double CpuUtilization { get; set; }
    public double Throughput { get; set; }
    public double AverageWaitingTime { get; set; }
    public double AverageTurnaroundTime { get; set; }
    public double AverageResponseTime { get; set; }
    public int ContextSwitchCount { get; set; }
    public double DeadlineMissRatio { get; set; }
    public int TotalProcesses { get; set; }
    public int CompletedProcesses { get; set; }

    public List<AdminSessionProcessDto> Processes { get; set; } = new();
    public List<AdminGanttEntryDto> GanttChart { get; set; } = new();
}

public class AdminSessionProcessDto
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
    public bool IsMissedDeadline { get; set; }
    public int StartTime { get; set; }
    public int EndTime { get; set; }
}

public class AdminGanttEntryDto
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public string Color { get; set; } = string.Empty;
    public bool IsContextSwitch { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserName { get; set; }
}

public class AdminFieldOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
