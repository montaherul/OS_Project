using ERDRR.Scheduling.Models;
using ERDRR.Scheduling.Strategies;
using FluentAssertions;
using Xunit;

namespace ERDRR.Tests.Scheduling;

public class RRSchedulerTests
{
    private readonly RRScheduler _scheduler;

    public RRSchedulerTests()
    {
        _scheduler = new RRScheduler(timeQuantum: 2);
    }

    [Fact]
    public void Execute_SingleProcess_CompletesProcess()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 4, Deadline = 10, RemainingTime = 4 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 2
        };

        var result = _scheduler.Execute(context);

        result.Processes.Should().HaveCount(1);
        result.Processes[0].IsCompleted.Should().BeTrue();
        result.Processes[0].CompletionTime.Should().Be(4);
    }

    [Fact]
    public void Execute_TwoProcesses_RoundRobins()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 4, Deadline = 10, RemainingTime = 4 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 4, Deadline = 10, RemainingTime = 4 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 2
        };

        var result = _scheduler.Execute(context);

        result.Processes.Should().HaveCount(2);
        result.Processes.Should().OnlyContain(p => p.IsCompleted);
        result.ContextSwitchCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Execute_TimeQuantum1_AlternatesEveryUnit()
    {
        var scheduler = new RRScheduler(timeQuantum: 1);
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 3, Deadline = 10, RemainingTime = 3 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 3, Deadline = 10, RemainingTime = 3 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 1
        };

        var result = scheduler.Execute(context);

        result.Processes.Should().OnlyContain(p => p.IsCompleted);
        result.GanttChart.Count.Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void Execute_ContextSwitching_Tracked()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 6, Deadline = 10, RemainingTime = 6 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 6, Deadline = 10, RemainingTime = 6 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 2
        };

        var result = _scheduler.Execute(context);

        result.ContextSwitchCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AlgorithmName_ReturnsRR()
    {
        _scheduler.AlgorithmName.Should().Be("RR");
    }
}
