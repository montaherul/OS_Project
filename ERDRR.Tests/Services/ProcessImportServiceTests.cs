using ERDRR.Models.DTOs;
using ERDRR.Models.Entities;
using ERDRR.Models.ViewModels;
using ERDRR.Repositories.Interfaces;
using ERDRR.Services.Implementations;
using ERDRR.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ERDRR.Tests.Services;

public class ProcessImportServiceTests
{
    private readonly ProcessImportService _service;
    private readonly Mock<IProcessService> _processServiceMock;
    private readonly Mock<IProcessRepository> _processRepoMock;
    private readonly Mock<ILogger<ProcessImportService>> _loggerMock;

    public ProcessImportServiceTests()
    {
        _processServiceMock = new Mock<IProcessService>();
        _processRepoMock = new Mock<IProcessRepository>();
        _loggerMock = new Mock<ILogger<ProcessImportService>>();
        _service = new ProcessImportService(
            _processServiceMock.Object,
            _processRepoMock.Object,
            _loggerMock.Object);
    }

    // ─── CSV Parsing Tests ───────────────────────────────

    [Fact]
    public async Task ParseCsvAsync_ValidFile_ReturnsRows()
    {
        var csv = "ProcessName,ArrivalTime,BurstTime,Deadline,Priority\nP1,0,5,10,1\nP2,1,3,8,1\n";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _service.ParseCsvAsync(stream);

        result.Should().HaveCount(2);
        result[0].ProcessName.Should().Be("P1");
        result[0].ArrivalTime.Should().Be(0);
        result[0].BurstTime.Should().Be(5);
        result[0].Deadline.Should().Be(10);
        result[0].Priority.Should().Be(1);
    }

    [Fact]
    public async Task ParseCsvAsync_EmptyFile_ReturnsEmpty()
    {
        var csv = "ProcessName,ArrivalTime,BurstTime,Deadline,Priority\n";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await _service.ParseCsvAsync(stream);

        result.Should().BeEmpty();
    }

    // ─── Excel Parsing Tests ─────────────────────────────

    [Fact]
    public async Task ParseExcelAsync_ValidFile_ReturnsRows()
    {
        var stream = CreateSampleExcelStream();

        var result = await _service.ParseExcelAsync(stream);

        result.Should().HaveCount(3);
        result[0].ProcessName.Should().Be("P1");
        result[1].ProcessName.Should().Be("P2");
        result[2].ProcessName.Should().Be("P3");
    }

    // ─── Validation Tests ────────────────────────────────

    [Fact]
    public void ValidateRows_AllValid_ReturnsNoErrors()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 1 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateRows_EmptyName_ReturnsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 1 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].Errors.Should().Contain(e => e.Contains("ProcessName"));
    }

    [Fact]
    public void ValidateRows_NegativeArrivalTime_ReturnsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = -1, BurstTime = 5, Deadline = 10, Priority = 1 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].Errors.Should().Contain(e => e.Contains("ArrivalTime"));
    }

    [Fact]
    public void ValidateRows_ZeroBurstTime_ReturnsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 0, Deadline = 10, Priority = 1 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].Errors.Should().Contain(e => e.Contains("BurstTime"));
    }

    [Fact]
    public void ValidateRows_ZeroDeadline_ReturnsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 0, Priority = 1 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].Errors.Should().Contain(e => e.Contains("Deadline"));
    }

    [Fact]
    public void ValidateRows_ZeroPriority_ReturnsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 0 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].Errors.Should().Contain(e => e.Contains("Priority"));
    }

    [Fact]
    public void ValidateRows_MultipleErrors_ReturnsAll()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "", ArrivalTime = -1, BurstTime = 0, Deadline = 0, Priority = 0 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].Errors.Should().HaveCount(5);
    }

    [Fact]
    public void ValidateRows_MixedRows_OnlyInvalidHaveErrors()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 1 },
            new() { ProcessName = "", ArrivalTime = 0, BurstTime = 0, Deadline = 0, Priority = 0 },
            new() { ProcessName = "P3", ArrivalTime = 2, BurstTime = 4, Deadline = 8, Priority = 2 }
        };

        var errors = _service.ValidateRows(rows);

        errors.Should().HaveCount(1);
        errors[0].RowNumber.Should().Be(3); // Row 3 (header=1, P1=2, P2=3)
    }

    // ─── SaveValidRowsAsync Tests ────────────────────────

    [Fact]
    public async Task SaveValidRowsAsync_EmptyUserId_SetsUserIdToNull()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 1 }
        };
        var errors = new List<ProcessImportRowError>();

        _processRepoMock
            .Setup(r => r.GetBySessionAndProcessIdAsync(1, "P1"))
            .ReturnsAsync((ProcessEntity?)null);

        _processServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateProcessDto>(), It.IsAny<string>()))
            .ReturnsAsync(new ProcessDto { Id = 1, Name = "P1", ProcessId = "P001" });

        var result = await _service.SaveValidRowsAsync(rows, errors, 1, "Test", "");

        result.ImportedRows.Should().Be(1);
        _processServiceMock.Verify(
            s => s.CreateAsync(It.IsAny<CreateProcessDto>(), ""),
            Times.Once);
    }

    [Fact]
    public async Task SaveValidRowsAsync_DuplicateName_ReturnsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 1 }
        };
        var errors = new List<ProcessImportRowError>();

        _processRepoMock
            .Setup(r => r.GetByNameAsync(1, "P1"))
            .ReturnsAsync((ProcessEntity?)new ProcessEntity { Name = "P1", ProcessId = "P1" });

        var result = await _service.SaveValidRowsAsync(rows, errors, 1, "Test", "user-1");

        result.ImportedRows.Should().Be(0);
        result.FailedRows.Should().Be(1);
        result.RowErrors.Should().Contain(e => e.Errors.Any(x => x.Contains("already exists")));
    }

    [Fact]
    public async Task SaveValidRowsAsync_DbError_CatchesAndReportsError()
    {
        var rows = new List<ProcessImportRow>
        {
            new() { ProcessName = "P1", ArrivalTime = 0, BurstTime = 5, Deadline = 10, Priority = 1 }
        };
        var errors = new List<ProcessImportRowError>();

        _processRepoMock
            .Setup(r => r.GetBySessionAndProcessIdAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((ProcessEntity?)null);

        _processServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateProcessDto>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _service.SaveValidRowsAsync(rows, errors, 1, "Test", "user-1");

        result.ImportedRows.Should().Be(0);
        result.FailedRows.Should().Be(1);
        result.RowErrors.Should().Contain(e => e.Errors.Any(x => x.Contains("DB failure")));
    }

    // ─── Sample File Generation Tests ────────────────────

    [Fact]
    public void GenerateSampleCsv_ReturnsValidBytes()
    {
        var bytes = _service.GenerateSampleCsv();

        bytes.Should().NotBeEmpty();
        var content = System.Text.Encoding.UTF8.GetString(bytes);
        content.Should().Contain("ProcessName,ArrivalTime,BurstTime,Deadline,Priority");
        content.Should().Contain("P1,0,5,10,1");
    }

    [Fact]
    public void GenerateSampleExcel_ReturnsValidBytes()
    {
        var bytes = _service.GenerateSampleExcel();

        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(0);
    }

    // ─── Helper ──────────────────────────────────────────

    private static MemoryStream CreateSampleExcelStream()
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Processes");
        ws.Cell(1, 1).Value = "ProcessName";
        ws.Cell(1, 2).Value = "ArrivalTime";
        ws.Cell(1, 3).Value = "BurstTime";
        ws.Cell(1, 4).Value = "Deadline";
        ws.Cell(1, 5).Value = "Priority";

        ws.Cell(2, 1).Value = "P1";
        ws.Cell(2, 2).Value = 0;
        ws.Cell(2, 3).Value = 5;
        ws.Cell(2, 4).Value = 10;
        ws.Cell(2, 5).Value = 1;

        ws.Cell(3, 1).Value = "P2";
        ws.Cell(3, 2).Value = 1;
        ws.Cell(3, 3).Value = 3;
        ws.Cell(3, 4).Value = 8;
        ws.Cell(3, 5).Value = 1;

        ws.Cell(4, 1).Value = "P3";
        ws.Cell(4, 2).Value = 2;
        ws.Cell(4, 3).Value = 4;
        ws.Cell(4, 4).Value = 8;
        ws.Cell(4, 5).Value = 1;

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }
}
