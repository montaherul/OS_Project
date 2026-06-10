using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERDRR.Models.Entities;

[Table("SchedulingResults")]
public class SchedulingResult : BaseEntity
{
    [Required]
    public int SchedulingSessionId { get; set; }

    [ForeignKey(nameof(SchedulingSessionId))]
    public SchedulingSession? SchedulingSession { get; set; }

    [Required]
    [MaxLength(100)]
    public string ProcessId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ProcessName { get; set; } = string.Empty;

    public int ArrivalTime { get; set; }

    public int BurstTime { get; set; }

    public int Deadline { get; set; }

    public int CompletionTime { get; set; }

    public int TurnaroundTime { get; set; }

    public int WaitingTime { get; set; }

    public int ResponseTime { get; set; }

    public bool IsMissedDeadline { get; set; }

    public int StartTime { get; set; }

    public int EndTime { get; set; }

    [MaxLength(2000)]
    public string? GanttChartData { get; set; }

    public double CpuUtilization { get; set; }

    public double Throughput { get; set; }

    public int ContextSwitchCount { get; set; }

    public double DeadlineMissRatio { get; set; }
}
