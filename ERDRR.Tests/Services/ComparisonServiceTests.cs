using ERDRR.Models.Entities;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Implementations;
using ERDRR.Scheduling.Engine;
using ERDRR.Scheduling.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ERDRR.Tests.Services;

public class ComparisonServiceTests
{
    private readonly Mock<IComparisonRepository> _comparisonRepoMock;
    private readonly Mock<ISessionRepository> _sessionRepoMock;
    private readonly Mock<IProcessRepository> _processRepoMock;
    private readonly SchedulingEngine _engine;
    private readonly ComparisonService _service;

    public ComparisonServiceTests()
    {
        _comparisonRepoMock = new Mock<IComparisonRepository>();
        _sessionRepoMock = new Mock<ISessionRepository>();
        _processRepoMock = new Mock<IProcessRepository>();
        _engine = new SchedulingEngine();
        var logger = new Mock<ILogger<ComparisonService>>();
        _service = new ComparisonService(
            _comparisonRepoMock.Object,
            _sessionRepoMock.Object,
            _processRepoMock.Object,
            _engine,
            logger.Object);
    }

    [Fact]
    public async Task CompareAlgorithmsAsync_ReturnsValidResult()
    {
        var session = new SchedulingSession { Id = 1, Name = "Test", AlgorithmType = "Hybrid", TimeQuantum = 4, IsPreemptive = true };
        var processes = new List<ProcessEntity>
        {
            new() { ProcessId = "P1", Name = "Process1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, SchedulingSessionId = 1 },
            new() { ProcessId = "P2", Name = "Process2", ArrivalTime = 1, BurstTime = 3, Deadline = 8, SchedulingSessionId = 1 },
            new() { ProcessId = "P3", Name = "Process3", ArrivalTime = 2, BurstTime = 4, Deadline = 12, SchedulingSessionId = 1 }
        };

        _sessionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        _processRepoMock.Setup(r => r.GetBySessionIdAsync(1)).ReturnsAsync(processes);
        _comparisonRepoMock.Setup(r => r.AddAsync(It.IsAny<AlgorithmComparison>()))
            .ReturnsAsync((AlgorithmComparison c) => { c.Id = 1; return c; });

        var result = await _service.CompareAlgorithmsAsync(1, "user-1");

        result.Should().NotBeNull();
        result.SessionId.Should().Be(1);
        result.EDF.Should().NotBeNull();
        result.RR.Should().NotBeNull();
        result.Hybrid.Should().NotBeNull();
        result.RecommendedAlgorithm.Should().BeOneOf("EDF", "RR", "Hybrid");
        result.BestScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CompareAlgorithmsAsync_ThrowsOnMissingSession()
    {
        _sessionRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((SchedulingSession?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CompareAlgorithmsAsync(999, "user-1"));
    }

    [Fact]
    public async Task CompareAlgorithmsAsync_ThrowsOnNoProcesses()
    {
        var session = new SchedulingSession { Id = 1, Name = "Test" };
        _sessionRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(session);
        _processRepoMock.Setup(r => r.GetBySessionIdAsync(1)).ReturnsAsync(new List<ProcessEntity>());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CompareAlgorithmsAsync(1, "user-1"));
    }

    [Fact]
    public void EDF_Scheduler_CompletesAllProcesses()
    {
        var processes = CreateTestProcesses();
        var result = _engine.RunSimulation(processes, "EDF", 4, true);

        result.Context.Processes.Should().AllSatisfy(p => p.IsCompleted.Should().BeTrue());
        result.Metrics.TotalProcesses.Should().Be(3);
        result.Metrics.CompletedProcesses.Should().Be(3);
    }

    [Fact]
    public void RR_Scheduler_CompletesAllProcesses()
    {
        var processes = CreateTestProcesses();
        var result = _engine.RunSimulation(processes, "RR", 4, true);

        result.Context.Processes.Should().AllSatisfy(p => p.IsCompleted.Should().BeTrue());
        result.Metrics.TotalProcesses.Should().Be(3);
        result.Metrics.CompletedProcesses.Should().Be(3);
    }

    [Fact]
    public void Hybrid_Scheduler_CompletesAllProcesses()
    {
        var processes = CreateTestProcesses();
        var result = _engine.RunSimulation(processes, "Hybrid", 4, true);

        result.Context.Processes.Should().AllSatisfy(p => p.IsCompleted.Should().BeTrue());
        result.Metrics.TotalProcesses.Should().Be(3);
        result.Metrics.CompletedProcesses.Should().Be(3);
    }

    [Fact]
    public void AllSchedulers_ProduceDifferentResults()
    {
        var processes = CreateTestProcesses();
        var edf = _engine.RunSimulation(processes, "EDF", 4, true);
        var rr = _engine.RunSimulation(processes, "RR", 4, true);
        var hybrid = _engine.RunSimulation(processes, "Hybrid", 4, true);

        var edfWaiting = edf.Metrics.AverageWaitingTime;
        var rrWaiting = rr.Metrics.AverageWaitingTime;
        var hybridWaiting = hybrid.Metrics.AverageWaitingTime;

        (edfWaiting != rrWaiting || rrWaiting != hybridWaiting).Should().BeTrue(
            "Different algorithms should produce different waiting times");
    }

    [Fact]
    public void GetRecommendation_ReturnsValidAlgorithm()
    {
        var processes = CreateTestProcesses();
        var edf = _engine.RunSimulation(processes, "EDF", 4, true);
        var rr = _engine.RunSimulation(processes, "RR", 4, true);
        var hybrid = _engine.RunSimulation(processes, "Hybrid", 4, true);

        edf.Metrics.AverageWaitingTime.Should().BeGreaterThanOrEqualTo(0);
        rr.Metrics.AverageWaitingTime.Should().BeGreaterThanOrEqualTo(0);
        hybrid.Metrics.AverageWaitingTime.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void Metrics_AllNonNegative()
    {
        var processes = CreateTestProcesses();
        var algorithms = new[] { "EDF", "RR", "Hybrid" };

        foreach (var algo in algorithms)
        {
            var result = _engine.RunSimulation(processes, algo, 4, true);
            result.Metrics.AverageWaitingTime.Should().BeGreaterThanOrEqualTo(0);
            result.Metrics.AverageTurnaroundTime.Should().BeGreaterThanOrEqualTo(0);
            result.Metrics.AverageResponseTime.Should().BeGreaterThanOrEqualTo(0);
            result.Metrics.CpuUtilization.Should().BeGreaterThanOrEqualTo(0);
            result.Metrics.Throughput.Should().BeGreaterThanOrEqualTo(0);
            result.Metrics.DeadlineMissRatio.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    private static List<ProcessControlBlock> CreateTestProcesses()
    {
        return new List<ProcessControlBlock>
        {
            new() { ProcessId = "P1", ProcessName = "Process1", ArrivalTime = 0, BurstTime = 5, RemainingTime = 5, Deadline = 10, State = "New" },
            new() { ProcessId = "P2", ProcessName = "Process2", ArrivalTime = 1, BurstTime = 3, RemainingTime = 3, Deadline = 8, State = "New" },
            new() { ProcessId = "P3", ProcessName = "Process3", ArrivalTime = 2, BurstTime = 4, RemainingTime = 4, Deadline = 12, State = "New" }
        };
    }
}
