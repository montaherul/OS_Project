using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EDFRR.Models.Entities;

[Table("SchedulingSessions")]
public class SchedulingSession : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    public string AlgorithmType { get; set; } = "Hybrid";

    [Range(1, 100)]
    public int TimeQuantum { get; set; } = 4;

    [MaxLength(20)]
    public string Status { get; set; } = "Created";

    public bool IsPreemptive { get; set; } = true;

    [MaxLength(450)]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    public ICollection<ProcessEntity> Processes { get; set; } = new List<ProcessEntity>();
    public ICollection<SchedulingResult> Results { get; set; } = new List<SchedulingResult>();
    public ICollection<ExecutionLog> ExecutionLogs { get; set; } = new List<ExecutionLog>();
}
