using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// Request DTO for Admin to create a user within their company
/// </summary>
public class CreateCompanyUserRequestDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Role to assign. Allowed values: LoanOfficer, CollectionsOfficer, Underwriter, SupportAgent, Auditor, Viewer
    /// </summary>
    [Required]
    public string Role { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
}

/// <summary>
/// Response DTO after creating a company user
/// </summary>
public class CreateCompanyUserResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CompanyUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? Role { get; set; }
}

public class CompanyUserFilterDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public string? Gender { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }
    public string? Role { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedCompanyUserListDto
{
    public List<CompanyUserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    
    // Summary statistics
    public int TotalActiveUsers { get; set; }
    public int TotalInactiveUsers { get; set; }
}
