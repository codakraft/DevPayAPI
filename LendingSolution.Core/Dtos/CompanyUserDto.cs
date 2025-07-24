namespace LendingSolution.Core.Dtos;

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
    
    // User statistics
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int PendingLoans { get; set; }
    public decimal TotalLoanAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
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
    public int? MinLoans { get; set; }
    public int? MaxLoans { get; set; }
    public decimal? MinLoanAmount { get; set; }
    public decimal? MaxLoanAmount { get; set; }
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
    public int TotalUsersWithLoans { get; set; }
    public decimal TotalLoanAmountAcrossUsers { get; set; }
    public decimal AverageLoanAmountPerUser { get; set; }
}
