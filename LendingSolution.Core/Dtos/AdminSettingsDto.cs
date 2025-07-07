namespace LendingSolution.Core.Dtos;

public class AdminSettingsDto
{
    public string Id { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateAdminSettingsDto
{
    public required string SettingKey { get; set; }
    public required string SettingValue { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class AdminListDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? CompanyId { get; set; }
    public string? Gender { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedAdminListDto
{
    public List<AdminListDto> Admins { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
