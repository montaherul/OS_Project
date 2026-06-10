using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERDRR.Models.Entities;

[Table("ExecutionLogs")]
public class ExecutionLog : BaseEntity
{
    [Required]
    public int SchedulingSessionId { get; set; }

    [ForeignKey(nameof(SchedulingSessionId))]
    public SchedulingSession? SchedulingSession { get; set; }

    public int TimeStep { get; set; }

    [Required]
    [MaxLength(100)]
    public string ExecutingProcessId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ExecutingProcessName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Action { get; set; } = "Execute";

    [MaxLength(500)]
    public string? Details { get; set; }

    public int QueueState { get; set; }

    [MaxLength(2000)]
    public string? ReadyQueueSnapshot { get; set; }
}
