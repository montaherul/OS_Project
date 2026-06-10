namespace ERDRR.Scheduling.Models;

public class GanttEntry
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int StartTime { get; set; }
    public int EndTime { get; set; }
    public bool IsContextSwitch { get; set; }
    public bool IsIdle { get; set; }

    public int Duration => EndTime - StartTime;
}

public class SchedulingMetrics
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
    public int TotalTime { get; set; }
}

public class SchedulingContext
{
    public List<ProcessControlBlock> Processes { get; set; } = new();
    public int TimeQuantum { get; set; } = 4;
    public bool IsPreemptive { get; set; } = true;
    public string AlgorithmType { get; set; } = "Hybrid";
    public List<GanttEntry> GanttChart { get; set; } = new();
    public List<ExecutionStep> ExecutionSteps { get; set; } = new();
    public int CurrentTime { get; set; }
    public int ContextSwitchCount { get; set; }
}

public class ExecutionStep
{
    public int TimeStep { get; set; }
    public string? ExecutingProcessId { get; set; }
    public string? ExecutingProcessName { get; set; }
    public string Action { get; set; } = "Execute";
    public string? Details { get; set; }
    public List<string> ReadyQueueSnapshot { get; set; } = new();
    public Dictionary<string, string> ProcessStates { get; set; } = new();
}
