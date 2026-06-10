using ERDRR.Models.DTOs;

namespace ERDRR.Models.ViewModels;

public class AdminDashboardViewModel
{
    public AdminDashboardDto Stats { get; set; } = new();
}

public class UserManagementViewModel
{
    public PagedResult<AdminUserDto> Users { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public string SortColumn { get; set; } = "CreatedAt";
    public string SortDirection { get; set; } = "DESC";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public List<string> AvailableRoles { get; set; } = new() { "Admin", "User" };
    public List<string> AvailableStatuses { get; set; } = new() { "Active", "Inactive", "Locked" };
    public List<string> SelectedUserIds { get; set; } = new();
}

public class UserDetailsViewModel
{
    public AdminUserDetailDto User { get; set; } = new();
}

public class EditUserViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; }
    public string CurrentRole { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
}

public class ResetPasswordViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
