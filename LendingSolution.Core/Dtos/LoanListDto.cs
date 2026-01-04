using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Dtos;

public class LoanListDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserFullName => $"{UserFirstName} {UserLastName}";
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public LoanStatus Status { get; set; }
    public string StatusDisplay => Status.ToString();
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Company Information
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyShortName { get; set; } = string.Empty;
    
    // Product Information
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductInterestRate { get; set; }
    
    // Additional Info
    public string Message { get; set; } = string.Empty;
    public bool IsMandateCreated { get; set; }
    public string MandateRef { get; set; } = string.Empty;
    
    // Document/Image Information
    public string? DocumentIds { get; set; } // Comma-separated document IDs from BorrowerApplication
    public Guid? OfferLetterDocumentId { get; set; }
    public string? OfferLetterUrl { get; set; }
    public Guid? SignedOfferLetterDocumentId { get; set; }
    
    // Salary History Information
    public SalaryHistoryInfoDto? SalaryHistory { get; set; }
}

/// <summary>
/// DTO for salary history information
/// </summary>
public class SalaryHistoryInfoDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SalaryCount { get; set; }
    public decimal AverageMonthlySalary { get; set; }
    public decimal LatestSalaryAmount { get; set; }
    public DateTime? LatestPaymentDate { get; set; }
    public decimal MinSalaryAmount { get; set; }
    public decimal MaxSalaryAmount { get; set; }
    public int ConsistentMonths { get; set; }
    public bool HasOutstandingLoans { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public DateTime? FirstPaymentDate { get; set; }
}

public class LoanFilterDto
{
    public string? Search { get; set; } // User name, email, loan purpose, account number
    public Guid? CompanyId { get; set; }
    public LoanStatus? Status { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int? MinDuration { get; set; }
    public int? MaxDuration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ApprovedAfter { get; set; }
    public DateTime? ApprovedBefore { get; set; }
    public Guid? ProductId { get; set; }
    public bool? IsMandateCreated { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedLoanListDto
{
    public List<LoanListDto> Loans { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    
    // Summary statistics
    public decimal TotalLoanAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}
