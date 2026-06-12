namespace EDFRR.Scheduling.Models;

public class ProcessControlBlock
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int RemainingTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
    public int CompletionTime { get; set; }
    public int StartTime { get; set; } = -1;
    public int FirstExecutionTime { get; set; } = -1;
    public string State { get; set; } = "New";
    public bool HasStarted { get; set; }
    public bool IsCompleted { get; set; }
    public bool MissedDeadline { get; set; }

    public int TurnaroundTime => CompletionTime - ArrivalTime;
    public int WaitingTime => TurnaroundTime - BurstTime;
    public int ResponseTime => FirstExecutionTime >= 0 ? FirstExecutionTime - ArrivalTime : 0;
    public int AbsoluteDeadline => ArrivalTime + Deadline;
    public bool IsExpired => !IsCompleted && CompletionTime > 0 && CompletionTime > AbsoluteDeadline;

    public ProcessControlBlock Clone()
    {
        return new ProcessControlBlock
        {
            ProcessId = ProcessId,
            ProcessName = ProcessName,
            ArrivalTime = ArrivalTime,
            BurstTime = BurstTime,
            RemainingTime = RemainingTime,
            Deadline = Deadline,
            Priority = Priority,
            CompletionTime = CompletionTime,
            StartTime = StartTime,
            FirstExecutionTime = FirstExecutionTime,
            State = State,
            HasStarted = HasStarted,
            IsCompleted = IsCompleted,
            MissedDeadline = MissedDeadline
        };
    }
}
