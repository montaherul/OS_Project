using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using EDFRR.Models.DTOs;
using EDFRR.Models.Entities;
using EDFRR.Models.ViewModels;
using EDFRR.Repositories.Interfaces;
using EDFRR.Services.Interfaces;

namespace EDFRR.Services.Implementations;

/// <summary>
/// Handles parsing, validation, and saving of imported process data from Excel and CSV files.
/// </summary>
public class ProcessImportService : IProcessImportService
{
    private readonly IProcessService _processService;
    private readonly IProcessRepository _processRepository;
    private readonly ILogger<ProcessImportService> _logger;

    public ProcessImportService(
        IProcessService processService,
        IProcessRepository processRepository,
        ILogger<ProcessImportService> logger)
    {
        _processService = processService;
        _processRepository = processRepository;
        _logger = logger;
    }

    /// <summary>
    /// Parses an Excel (.xlsx) stream into ProcessImportRow objects.
    /// Expects columns: ProcessName, ArrivalTime, BurstTime, Deadline, Priority
    /// </summary>
    public async Task<List<ProcessImportRow>> ParseExcelAsync(Stream fileStream)
    {
        _logger.LogInformation("Parsing Excel file stream.");

        return await Task.Run(() =>
        {
            var rows = new List<ProcessImportRow>();

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheets.First();
            var range = worksheet.RangeUsed();

            if (range == null || range.RowCount() < 2)
            {
                _logger.LogWarning("Excel file is empty or has no data rows.");
                return rows;
            }

            for (int row = 2; row <= range.RowCount(); row++)
            {
                var name = worksheet.Cell(row, 1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    _logger.LogWarning("Skipping empty row {Row}.", row);
                    continue;
                }

                rows.Add(new ProcessImportRow
                {
                    ProcessName = name,
                    ArrivalTime = ParseInt(worksheet.Cell(row, 2).GetString()),
                    BurstTime = ParseInt(worksheet.Cell(row, 3).GetString()),
                    Deadline = ParseInt(worksheet.Cell(row, 4).GetString()),
                    Priority = ParseInt(worksheet.Cell(row, 5).GetString())
                });
            }

            _logger.LogInformation("Parsed {Count} rows from Excel file.", rows.Count);
            return rows;
        });
    }

    /// <summary>
    /// Parses a CSV stream into ProcessImportRow objects using CsvHelper.
    /// Expects headers: ProcessName, ArrivalTime, BurstTime, Deadline, Priority
    /// </summary>
    public async Task<List<ProcessImportRow>> ParseCsvAsync(Stream fileStream)
    {
        _logger.LogInformation("Parsing CSV file stream.");

        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim()
        });

        var records = new List<ProcessImportRow>();

        await foreach (var record in csv.GetRecordsAsync<ProcessImportRow>())
        {
            records.Add(record);
        }

        _logger.LogInformation("Parsed {Count} rows from CSV file.", records.Count);
        return records;
    }

    /// <summary>
    /// Validates each row against business rules:
    /// - ProcessName required
    /// - ArrivalTime >= 0
    /// - BurstTime > 0
    /// - Deadline > 0
    /// - Priority > 0
    /// </summary>
    public List<ProcessImportRowError> ValidateRows(List<ProcessImportRow> rows)
    {
        _logger.LogInformation("Validating {Count} rows.", rows.Count);

        var errors = new List<ProcessImportRowError>();

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var rowErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(row.ProcessName))
                rowErrors.Add("ProcessName is required.");

            if (row.ArrivalTime < 0)
                rowErrors.Add("ArrivalTime must be >= 0.");

            if (row.BurstTime <= 0)
                rowErrors.Add("BurstTime must be > 0.");

            if (row.Deadline <= 0)
                rowErrors.Add("Deadline must be > 0.");

            if (row.Priority <= 0)
                rowErrors.Add("Priority must be > 0.");

            if (rowErrors.Count > 0)
            {
                errors.Add(new ProcessImportRowError
                {
                    RowNumber = i + 2, // +2 because row 1 is header, and 0-indexed
                    ProcessName = row.ProcessName,
                    Errors = rowErrors
                });
            }
        }

        _logger.LogInformation("Validation complete: {Valid} valid, {Invalid} invalid rows.",
            rows.Count - errors.Count, errors.Count);

        return errors;
    }

    /// <summary>
    /// Saves all valid rows (rows not in the error list) to the database.
    /// Auto-generates ProcessId for each saved record.
    /// Includes duplicate detection and per-row error handling.
    /// </summary>
    public async Task<ProcessImportResult> SaveValidRowsAsync(
        List<ProcessImportRow> rows,
        List<ProcessImportRowError> errors,
        int schedulingSessionId,
        string sessionName,
        string userId)
    {
        _logger.LogInformation(
            "Starting import: SessionId={SessionId}, TotalRows={Total}, PreValidErrors={Errors}",
            schedulingSessionId, rows.Count, errors.Count);

        // Fix: convert empty string userId to null for FK compliance
        var validUserId = string.IsNullOrWhiteSpace(userId) ? null : userId;

        var failedRowNumbers = new HashSet<int>(errors.Select(e => e.RowNumber));
        var importedRowNumbers = new HashSet<int>();
        var importedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var runtimeErrorRows = new List<ProcessImportRowError>();
        int importedCount = 0;

        for (int i = 0; i < rows.Count; i++)
        {
            int rowNumber = i + 2;
            if (failedRowNumbers.Contains(rowNumber))
                continue;

            var row = rows[i];

            try
            {
                // 1) In-batch duplicate: same name already imported in this loop
                if (importedNames.Contains(row.ProcessName))
                {
                    _logger.LogWarning(
                        "Row {Row}: Duplicate process name '{Name}' within import batch. Skipping.",
                        rowNumber, row.ProcessName);

                    runtimeErrorRows.Add(new ProcessImportRowError
                    {
                        RowNumber = rowNumber,
                        ProcessName = row.ProcessName,
                        Errors = new List<string> { $"Duplicate process name '{row.ProcessName}' already imported in this batch." }
                    });
                    continue;
                }

                // 2) Database duplicate: check by NAME
                var existingByName = await _processRepository.GetByNameAsync(
                    schedulingSessionId, row.ProcessName);

                if (existingByName != null)
                {
                    _logger.LogWarning(
                        "Row {Row}: Process name '{Name}' already exists in session {SessionId}. Skipping.",
                        rowNumber, row.ProcessName, schedulingSessionId);

                    runtimeErrorRows.Add(new ProcessImportRowError
                    {
                        RowNumber = rowNumber,
                        ProcessName = row.ProcessName,
                        Errors = new List<string> { $"A process named '{row.ProcessName}' already exists in this session." }
                    });
                    continue;
                }

                var dto = new CreateProcessDto
                {
                    Name = row.ProcessName,
                    ArrivalTime = row.ArrivalTime,
                    BurstTime = row.BurstTime,
                    Deadline = row.Deadline,
                    Priority = row.Priority,
                    SchedulingSessionId = schedulingSessionId
                };

                await _processService.CreateAsync(dto, validUserId ?? string.Empty);
                importedCount++;
                importedRowNumbers.Add(rowNumber);
                importedNames.Add(row.ProcessName);

                _logger.LogInformation(
                    "Row {Row}: Imported process '{Name}' successfully.",
                    rowNumber, row.ProcessName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Row {Row}: Failed to import process '{Name}'.",
                    rowNumber, row.ProcessName);

                runtimeErrorRows.Add(new ProcessImportRowError
                {
                    RowNumber = rowNumber,
                    ProcessName = row.ProcessName,
                    Errors = new List<string> { $"Database error: {ex.InnerException?.Message ?? ex.Message}" }
                });
            }
        }

        var allErrors = errors.Concat(runtimeErrorRows).OrderBy(e => e.RowNumber).ToList();
        int failedCount = allErrors.Count;

        _logger.LogInformation(
            "Import complete: SessionId={SessionId}, Imported={Imported}, Failed={Failed}",
            schedulingSessionId, importedCount, failedCount);

        return new ProcessImportResult
        {
            TotalRows = rows.Count,
            ImportedRows = importedCount,
            FailedRows = failedCount,
            SchedulingSessionId = schedulingSessionId,
            SessionName = sessionName,
            RowErrors = allErrors,
            ImportedData = rows.Where((_, i) => importedRowNumbers.Contains(i + 2)).ToList()
        };
    }

    /// <summary>
    /// Generates a sample CSV file as bytes for download.
    /// </summary>
    public byte[] GenerateSampleCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("ProcessName,ArrivalTime,BurstTime,Deadline,Priority");
        sb.AppendLine("P1,0,5,10,1");
        sb.AppendLine("P2,1,3,8,1");
        sb.AppendLine("P3,2,4,8,1");
        sb.AppendLine("P4,3,2,12,1");
        sb.AppendLine("P5,0,6,15,2");
        sb.AppendLine("P6,4,3,10,1");
        sb.AppendLine("P7,2,5,14,3");
        sb.AppendLine("P8,5,4,12,2");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Generates a sample Excel file as bytes for download.
    /// </summary>
    public byte[] GenerateSampleExcel()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Processes");

        // Headers
        worksheet.Cell(1, 1).Value = "ProcessName";
        worksheet.Cell(1, 2).Value = "ArrivalTime";
        worksheet.Cell(1, 3).Value = "BurstTime";
        worksheet.Cell(1, 4).Value = "Deadline";
        worksheet.Cell(1, 5).Value = "Priority";

        // Bold headers
        worksheet.Row(1).Style.Font.Bold = true;

        // Sample data
        var sampleData = new[]
        {
            ("P1", 0, 5, 10, 1),
            ("P2", 1, 3, 8, 1),
            ("P3", 2, 4, 8, 1),
            ("P4", 3, 2, 12, 1),
            ("P5", 0, 6, 15, 2),
            ("P6", 4, 3, 10, 1),
            ("P7", 2, 5, 14, 3),
            ("P8", 5, 4, 12, 2)
        };

        for (int i = 0; i < sampleData.Length; i++)
        {
            var (name, arrival, burst, deadline, priority) = sampleData[i];
            worksheet.Cell(i + 2, 1).Value = name;
            worksheet.Cell(i + 2, 2).Value = arrival;
            worksheet.Cell(i + 2, 3).Value = burst;
            worksheet.Cell(i + 2, 4).Value = deadline;
            worksheet.Cell(i + 2, 5).Value = priority;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Safely parses a string to int, returning 0 on failure.
    /// </summary>
    private static int ParseInt(string? value)
    {
        if (int.TryParse(value, out int result))
            return result;
        return 0;
    }
}
