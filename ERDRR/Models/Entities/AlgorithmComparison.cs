using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERDRR.Models.Entities;

[Table("AlgorithmComparisons")]
public class AlgorithmComparison : BaseEntity
{
    [Required]
    public int SchedulingSessionId { get; set; }

    [ForeignKey(nameof(SchedulingSessionId))]
    public SchedulingSession? SchedulingSession { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    // EDF Metrics
    public double EDFWaitingTime { get; set; }
    public double EDFTurnaroundTime { get; set; }
    public double EDFResponseTime { get; set; }
    public double EDFCPUUtilization { get; set; }
    public double EDFThroughput { get; set; }
    public int EDFContextSwitches { get; set; }
    public double EDFDeadlineMissRatio { get; set; }
    public int EDFExecutionTime { get; set; }

    // RR Metrics
    public double RRWaitingTime { get; set; }
    public double RRTurnaroundTime { get; set; }
    public double RRResponseTime { get; set; }
    public double RRCPUUtilization { get; set; }
    public double RRThroughput { get; set; }
    public int RRContextSwitches { get; set; }
    public double RRDeadlineMissRatio { get; set; }
    public int RRExecutionTime { get; set; }

    // Hybrid Metrics
    public double HybridWaitingTime { get; set; }
    public double HybridTurnaroundTime { get; set; }
    public double HybridResponseTime { get; set; }
    public double HybridCPUUtilization { get; set; }
    public double HybridThroughput { get; set; }
    public int HybridContextSwitches { get; set; }
    public double HybridDeadlineMissRatio { get; set; }
    public int HybridExecutionTime { get; set; }

    [MaxLength(50)]
    public string RecommendedAlgorithm { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? RecommendationReason { get; set; }

    public double BestScore { get; set; }
}
