using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERDRR.Models.Entities;

[Table("ActivityLogs")]
public class ActivityLog
{
    [Key]
    public int Id { get; set; }

    [MaxLength(450)]
    public string? UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? IPAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
