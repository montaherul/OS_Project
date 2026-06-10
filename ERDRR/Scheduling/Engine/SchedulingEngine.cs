using ERDRR.Scheduling.Interfaces;
using ERDRR.Scheduling.Models;
using ERDRR.Scheduling.Strategies;

namespace ERDRR.Scheduling.Engine;

public class SchedulingEngine
{
    private IScheduler CreateScheduler(string algorithmType, int timeQuantum, bool isPreemptive)
    {
        return algorithmType.ToLower() switch
        {
            "edf" => new EDFScheduler(isPreemptive),
            "rr" => new RRScheduler(timeQuantum),
            "hybrid" => new HybridEDFRRScheduler(timeQuantum, isPreemptive),
            _ => new HybridEDFRRScheduler(timeQuantum, isPreemptive)
        };
    }

    public SchedulingResult RunSimulation(
        List<ProcessControlBlock> processes,
        string algorithmType,
        int timeQuantum,
        bool isPreemptive)
    {
        var context = new SchedulingContext
        {
            Processes = processes.Select(p => p.Clone()).ToList(),
            AlgorithmType = algorithmType,
            TimeQuantum = timeQuantum,
            IsPreemptive = isPreemptive
        };

        var scheduler = CreateScheduler(algorithmType, timeQuantum, isPreemptive);
        var result = scheduler.Execute(context);

        var metrics = CalculateMetrics(result);

        return new SchedulingResult
        {
            Context = result,
            Metrics = metrics
        };
    }

    public SchedulingMetrics CalculateMetrics(SchedulingContext context)
    {
        var completedProcesses = context.Processes.Where(p => p.IsCompleted).ToList();
        int totalProcesses = context.Processes.Count;

        double avgWaitingTime = completedProcesses.Count > 0
            ? completedProcesses.Average(p => p.WaitingTime)
            : 0;

        double avgTurnaroundTime = completedProcesses.Count > 0
            ? completedProcesses.Average(p => p.TurnaroundTime)
            : 0;

        double avgResponseTime = completedProcesses.Count > 0
            ? completedProcesses.Average(p => p.ResponseTime)
            : 0;

        int busyTime = context.GanttChart
            .Where(g => !g.IsIdle)
            .Sum(g => g.Duration);

        double cpuUtilization = context.CurrentTime > 0
            ? (double)busyTime / context.CurrentTime * 100
            : 0;

        double throughput = context.CurrentTime > 0
            ? (double)completedProcesses.Count / context.CurrentTime
            : 0;

        int missedDeadlines = completedProcesses.Count(p => p.MissedDeadline);
        double deadlineMissRatio = totalProcesses > 0
            ? (double)missedDeadlines / totalProcesses * 100
            : 0;

        return new SchedulingMetrics
        {
            AverageWaitingTime = Math.Round(avgWaitingTime, 2),
            AverageTurnaroundTime = Math.Round(avgTurnaroundTime, 2),
            AverageResponseTime = Math.Round(avgResponseTime, 2),
            CpuUtilization = Math.Round(cpuUtilization, 2),
            Throughput = Math.Round(throughput, 4),
            ContextSwitchCount = context.ContextSwitchCount,
            MissedDeadlines = missedDeadlines,
            DeadlineMissRatio = Math.Round(deadlineMissRatio, 2),
            TotalProcesses = totalProcesses,
            CompletedProcesses = completedProcesses.Count,
            TotalTime = context.CurrentTime
        };
    }
}

public class SchedulingResult
{
    public SchedulingContext Context { get; set; } = new();
    public SchedulingMetrics Metrics { get; set; } = new();
}
