using ERDRR.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERDRR.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProcessEntity> Processes => Set<ProcessEntity>();
    public DbSet<SchedulingSession> SchedulingSessions => Set<SchedulingSession>();
    public DbSet<SchedulingResult> SchedulingResults => Set<SchedulingResult>();
    public DbSet<ExecutionLog> ExecutionLogs => Set<ExecutionLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        builder.Entity<ProcessEntity>(entity =>
        {
            entity.HasIndex(e => new { e.SchedulingSessionId, e.ProcessId }).IsUnique();
            entity.HasOne(e => e.SchedulingSession)
                .WithMany(s => s.Processes)
                .HasForeignKey(e => e.SchedulingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany(u => u.Processes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SchedulingSession>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.SchedulingSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<SchedulingResult>(entity =>
        {
            entity.HasIndex(e => e.SchedulingSessionId);
            entity.HasOne(e => e.SchedulingSession)
                .WithMany(s => s.Results)
                .HasForeignKey(e => e.SchedulingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ExecutionLog>(entity =>
        {
            entity.HasIndex(e => e.SchedulingSessionId);
            entity.HasOne(e => e.SchedulingSession)
                .WithMany(s => s.ExecutionLogs)
                .HasForeignKey(e => e.SchedulingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        var adminId = "8e445865-e2ff-4350-84d0-4c83e07bf1f3";
        var userId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        builder.Entity<ApplicationUser>().HasData(
            new ApplicationUser
            {
                Id = adminId,
                UserName = "admin@erdr.com",
                NormalizedUserName = "ADMIN@ERDRR.COM",
                Email = "admin@erdr.com",
                NormalizedEmail = "ADMIN@ERDRR.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEOj45gtf5V6AJ+8RVY4fOo2GutZGEX8a89CbleFQjF2F8lpFNnhU2bTVM/nDl8jFHQ==",
                SecurityStamp = "ADMIN_SECURITY_STAMP_001",
                ConcurrencyStamp = "admin-concurrency-stamp",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            },
            new ApplicationUser
            {
                Id = userId,
                UserName = "user@erdr.com",
                NormalizedUserName = "USER@ERDRR.COM",
                Email = "user@erdr.com",
                NormalizedEmail = "USER@ERDRR.COM",
                EmailConfirmed = true,
                PasswordHash = "AQAAAAIAAYagAAAAEF5uLQNal2ETe7OHuocUE8wOslEw/U7GN9LM+lwpSj/jrcy+s0kI7BLKcwuCOtxXRQ==",
                SecurityStamp = "USER_SECURITY_STAMP_001",
                ConcurrencyStamp = "user-concurrency-stamp",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnd = null,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            }
        );

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "admin-role-id-001",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "admin-role-stamp"
            },
            new IdentityRole
            {
                Id = "user-role-id-001",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "user-role-stamp"
            }
        );

        builder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                UserId = adminId,
                RoleId = "admin-role-id-001"
            },
            new IdentityUserRole<string>
            {
                UserId = userId,
                RoleId = "user-role-id-001"
            }
        );
    }
}
