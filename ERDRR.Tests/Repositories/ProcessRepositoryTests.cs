using ERDRR.Data;
using ERDRR.Models.Entities;
using ERDRR.Repositories.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ERDRR.Tests.Repositories;

public class ProcessRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProcessRepository _repository;

    public ProcessRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ProcessRepository(_context);
    }

    [Fact]
    public async Task AddAsync_AddsProcessSuccessfully()
    {
        var process = new ProcessEntity
        {
            Name = "Test Process",
            ProcessId = "P001",
            ArrivalTime = 0,
            BurstTime = 5,
            Deadline = 10,
            SchedulingSessionId = 1,
            Status = "Pending"
        };

        var result = await _repository.AddAsync(process);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.ProcessId.Should().Be("P001");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProcess()
    {
        var process = new ProcessEntity
        {
            Name = "Test Process",
            ProcessId = "P001",
            ArrivalTime = 0,
            BurstTime = 5,
            Deadline = 10,
            SchedulingSessionId = 1,
            Status = "Pending"
        };

        await _repository.AddAsync(process);

        var result = await _repository.GetByIdAsync(process.Id);

        result.Should().NotBeNull();
        result!.ProcessId.Should().Be("P001");
    }

    [Fact]
    public async Task CountBySessionIdAsync_ReturnsCorrectCount()
    {
        await _repository.AddAsync(new ProcessEntity { Name = "P1", ProcessId = "P001", ArrivalTime = 0, BurstTime = 5, Deadline = 10, SchedulingSessionId = 1, Status = "Pending" });
        await _repository.AddAsync(new ProcessEntity { Name = "P2", ProcessId = "P002", ArrivalTime = 0, BurstTime = 3, Deadline = 8, SchedulingSessionId = 1, Status = "Pending" });
        await _repository.AddAsync(new ProcessEntity { Name = "P3", ProcessId = "P003", ArrivalTime = 0, BurstTime = 4, Deadline = 12, SchedulingSessionId = 2, Status = "Pending" });

        var count = await _repository.CountBySessionIdAsync(1);

        count.Should().Be(2);
    }
}
