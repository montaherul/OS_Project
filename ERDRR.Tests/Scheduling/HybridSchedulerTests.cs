using ERDRR.Scheduling.Engine;
using ERDRR.Scheduling.Models;
using FluentAssertions;
using Xunit;

namespace ERDRR.Tests.Scheduling;

public class HybridSchedulerTests
{
    private readonly SchedulingEngine _engine;

    public HybridSchedulerTests()
    {
        _engine = new SchedulingEngine();
    }

    [Fact]
    public void RunSimulation_HybridAlgorithm_CompletesAllProcesses()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, RemainingTime = 5 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 1, BurstTime = 3, Deadline = 8, RemainingTime = 3 },
            new() { ProcessId = "P3", ProcessName = "P3", ArrivalTime = 2, BurstTime = 4, Deadline = 12, RemainingTime = 4 }
        };

        var result = _engine.RunSimulation(processes, "Hybrid", 4, true);

        result.Metrics.TotalProcesses.Should().Be(3);
        result.Metrics.CompletedProcesses.Should().Be(3);
        result.Context.GanttChart.Should().NotBeEmpty();
    }

    [Fact]
    public void RunSimulation_EDFAlgorithm_CompletesAllProcesses()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 4, Deadline = 10, RemainingTime = 4 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 3, Deadline = 6, RemainingTime = 3 }
        };

        var result = _engine.RunSimulation(processes, "EDF", 4, true);

        result.Metrics.TotalProcesses.Should().Be(2);
        result.Metrics.CompletedProcesses.Should().Be(2);
    }

    [Fact]
    public void RunSimulation_RRAlgorithm_CompletesAllProcesses()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 4, Deadline = 10, RemainingTime = 4 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 4, Deadline = 10, RemainingTime = 4 }
        };

        var result = _engine.RunSimulation(processes, "RR", 2, true);

        result.Metrics.TotalProcesses.Should().Be(2);
        result.Metrics.CompletedProcesses.Should().Be(2);
    }

    [Fact]
    public void CalculateMetrics_CorrectAverages()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, RemainingTime = 5, CompletionTime = 5, IsCompleted = true },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 3, Deadline = 8, RemainingTime = 3, CompletionTime = 8, IsCompleted = true }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            GanttChart = new List<GanttEntry>
            {
                new() { ProcessId = "P1", ProcessName = "P1", StartTime = 0, EndTime = 5 },
                new() { ProcessId = "P2", ProcessName = "P2", StartTime = 5, EndTime = 8 }
            },
            CurrentTime = 8,
            ContextSwitchCount = 1
        };

        var metrics = _engine.CalculateMetrics(context);

        metrics.AverageWaitingTime.Should().Be(2.5);
        metrics.AverageTurnaroundTime.Should().Be(6.5);
        metrics.TotalProcesses.Should().Be(2);
        metrics.CompletedProcesses.Should().Be(2);
    }

    [Fact]
    public void RunSimulation_DetectsMissedDeadlines()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 15, Deadline = 5, RemainingTime = 15 }
        };

        var result = _engine.RunSimulation(processes, "Hybrid", 4, true);

        result.Metrics.MissedDeadlines.Should().Be(1);
    }

    [Fact]
    public void RunSimulation_WithDynamicArrivals_HandlesCorrectly()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 3, Deadline = 8, RemainingTime = 3 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 5, BurstTime = 3, Deadline = 12, RemainingTime = 3 }
        };

        var result = _engine.RunSimulation(processes, "Hybrid", 2, true);

        result.Metrics.TotalProcesses.Should().Be(2);
        result.Metrics.CompletedProcesses.Should().Be(2);
        result.Context.CurrentTime.Should().BeGreaterThanOrEqualTo(5);
    }
}
