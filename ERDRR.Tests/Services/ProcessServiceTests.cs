using ERDRR.Data;
using ERDRR.Models.Entities;
using ERDRR.Repositories.Implementations;
using ERDRR.Services.Implementations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ERDRR.Tests.Services;

public class ProcessServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly ProcessRepository _processRepository;
    private readonly ProcessService _service;

    public ProcessServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _processRepository = new ProcessRepository(_context);
        _service = new ProcessService(_processRepository);
    }

    [Fact]
    public async Task CreateAsync_CreatesProcessSuccessfully()
    {
        var dto = new ERDRR.Models.DTOs.CreateProcessDto
        {
            Name = "Test Process",
            ArrivalTime = 0,
            BurstTime = 5,
            Deadline = 10,
            Priority = 0,
            SchedulingSessionId = 1
        };

        var result = await _service.CreateAsync(dto, "user1");

        result.Should().NotBeNull();
        result.ProcessId.Should().Be("P001");
        result.Name.Should().Be("Test Process");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProcess()
    {
        var dto = new ERDRR.Models.DTOs.CreateProcessDto
        {
            Name = "Test Process",
            ArrivalTime = 0,
            BurstTime = 5,
            Deadline = 10,
            Priority = 0,
            SchedulingSessionId = 1
        };

        var created = await _service.CreateAsync(dto, "user1");
        var result = await _service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Process");
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesProcess()
    {
        var dto = new ERDRR.Models.DTOs.CreateProcessDto
        {
            Name = "Test Process",
            ArrivalTime = 0,
            BurstTime = 5,
            Deadline = 10,
            Priority = 0,
            SchedulingSessionId = 1
        };

        var created = await _service.CreateAsync(dto, "user1");
        var result = await _service.DeleteAsync(created.Id);

        result.Should().BeTrue();
        var entity = await _processRepository.GetByIdAsync(created.Id);
        entity.Should().NotBeNull();
        entity!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        await _service.CreateAsync(new ERDRR.Models.DTOs.CreateProcessDto { Name = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, SchedulingSessionId = 1 }, "user1");
        await _service.CreateAsync(new ERDRR.Models.DTOs.CreateProcessDto { Name = "P2", ArrivalTime = 0, BurstTime = 3, Deadline = 8, SchedulingSessionId = 1 }, "user1");

        var count = await _service.GetTotalCountAsync();

        count.Should().Be(2);
    }
}
