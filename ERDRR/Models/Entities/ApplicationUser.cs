using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ERDRR.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    public ICollection<ProcessEntity> Processes { get; set; } = new List<ProcessEntity>();
    public ICollection<SchedulingSession> SchedulingSessions { get; set; } = new List<SchedulingSession>();
}
