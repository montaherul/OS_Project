using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERDRR.Models.Entities;

[Table("Processes")]
public class ProcessEntity : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ProcessId { get; set; } = string.Empty;

    [Required]
    [Range(0, int.MaxValue)]
    public int ArrivalTime { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int BurstTime { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Deadline { get; set; }

    [Range(0, 10)]
    public int Priority { get; set; } = 0;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required]
    public int SchedulingSessionId { get; set; }

    [ForeignKey(nameof(SchedulingSessionId))]
    public SchedulingSession? SchedulingSession { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
}
