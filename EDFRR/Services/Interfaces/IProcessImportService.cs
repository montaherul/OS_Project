namespace EDFRR.Services.Interfaces;

/// <summary>
/// Service for importing process data from Excel and CSV files.
/// </summary>
public interface IProcessImportService
{
    /// <summary>
    /// Parses an uploaded Excel (.xlsx) file into a list of ProcessImportRow.
    /// </summary>
    Task<List<Models.ViewModels.ProcessImportRow>> ParseExcelAsync(Stream fileStream);

    /// <summary>
    /// Parses an uploaded CSV (.csv) file into a list of ProcessImportRow.
    /// </summary>
    Task<List<Models.ViewModels.ProcessImportRow>> ParseCsvAsync(Stream fileStream);

    /// <summary>
    /// Validates a list of parsed rows against business rules.
    /// Returns rows with their validation errors.
    /// </summary>
    List<Models.ViewModels.ProcessImportRowError> ValidateRows(List<Models.ViewModels.ProcessImportRow> rows);

    /// <summary>
    /// Saves all valid rows to the database, skipping invalid ones.
    /// Returns the import result summary.
    /// </summary>
    Task<Models.ViewModels.ProcessImportResult> SaveValidRowsAsync(
        List<Models.ViewModels.ProcessImportRow> rows,
        List<Models.ViewModels.ProcessImportRowError> errors,
        int schedulingSessionId,
        string sessionName,
        string userId);

    /// <summary>
    /// Generates a sample CSV file as a byte array for download.
    /// </summary>
    byte[] GenerateSampleCsv();

    /// <summary>
    /// Generates a sample Excel file as a byte array for download.
    /// </summary>
    byte[] GenerateSampleExcel();
}
