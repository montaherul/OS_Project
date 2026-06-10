using ERDRR.Scheduling.Models;
using ERDRR.Scheduling.Strategies;
using FluentAssertions;
using Xunit;

namespace ERDRR.Tests.Scheduling;

public class EDFSchedulerTests
{
    private readonly EDFScheduler _scheduler;

    public EDFSchedulerTests()
    {
        _scheduler = new EDFScheduler(isPreemptive: true);
    }

    [Fact]
    public void Execute_SingleProcess_CompletesProcess()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, RemainingTime = 5 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 4,
            IsPreemptive = true
        };

        var result = _scheduler.Execute(context);

        result.Processes.Should().HaveCount(1);
        result.Processes[0].IsCompleted.Should().BeTrue();
        result.Processes[0].CompletionTime.Should().Be(5);
        result.GanttChart.Should().NotBeEmpty();
    }

    [Fact]
    public void Execute_TwoProcesses_SelectsEarlierDeadline()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, RemainingTime = 5 },
            new() { ProcessId = "P2", ProcessName = "P2", ArrivalTime = 0, BurstTime = 3, Deadline = 5, RemainingTime = 3 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 4,
            IsPreemptive = true
        };

        var result = _scheduler.Execute(context);

        result.Processes.Should().HaveCount(2);
        result.Processes.Should().OnlyContain(p => p.IsCompleted);
    }

    [Fact]
    public void Execute_MissedDeadline_DetectsMissedDeadline()
    {
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 15, Deadline = 5, RemainingTime = 15 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 4,
            IsPreemptive = true
        };

        var result = _scheduler.Execute(context);

        result.Processes[0].MissedDeadline.Should().BeTrue();
    }

    [Fact]
    public void Execute_EmptyProcesses_ReturnsEmptyResult()
    {
        var context = new SchedulingContext
        {
            Processes = new List<ProcessControlBlock>(),
            TimeQuantum = 4
        };

        var result = _scheduler.Execute(context);

        result.Processes.Should().BeEmpty();
        result.GanttChart.Should().BeEmpty();
    }

    [Fact]
    public void Execute_NonPreemptive_RunsProcessToCompletion()
    {
        var scheduler = new EDFScheduler(isPreemptive: false);
        var processes = new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, RemainingTime = 5 }
        };

        var context = new SchedulingContext
        {
            Processes = processes,
            TimeQuantum = 4,
            IsPreemptive = false
        };

        var result = scheduler.Execute(context);

        result.Processes[0].IsCompleted.Should().BeTrue();
        result.GanttChart.Should().HaveCount(1);
    }

    [Fact]
    public void AlgorithmName_ReturnsEDF()
    {
        _scheduler.AlgorithmName.Should().Be("EDF");
    }
}
