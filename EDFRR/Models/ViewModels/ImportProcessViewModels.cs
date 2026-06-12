using System.ComponentModel.DataAnnotations;

namespace EDFRR.Models.ViewModels;

/// <summary>
/// View model for the Process Import page.
/// Holds the uploaded file and session context.
/// </summary>
public class ImportProcessViewModel
{
    [Required(ErrorMessage = "Please select a file to upload.")]
    public IFormFile? File { get; set; }

    public int SchedulingSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single row parsed from the uploaded file before validation.
/// </summary>
public class ProcessImportRow
{
    public string ProcessName { get; set; } = string.Empty;
    public int ArrivalTime { get; set; }
    public int BurstTime { get; set; }
    public int Deadline { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Represents a validation error for a single imported row.
/// </summary>
public class ProcessImportRowError
{
    public int RowNumber { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of an import operation â€” summary counts and error details.
/// </summary>
public class ProcessImportResult
{
    public int TotalRows { get; set; }
    public int ImportedRows { get; set; }
    public int FailedRows { get; set; }
    public int SchedulingSessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public List<ProcessImportRowError> RowErrors { get; set; } = new();
    public List<ProcessImportRow> ImportedData { get; set; } = new();
    public bool HasErrors => FailedRows > 0;
}
