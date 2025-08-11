using LendingSolution.Core.Models;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO for viewing salary history details
/// </summary>
public class SalaryHistoryViewDto
{
    public Guid Id { get; set; }
    public Guid BorrowerApplicationId { get; set; }
    public string BorrowerEmail { get; set; } = string.Empty;
    public string BorrowerFullName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string BVN { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public decimal TotalSalaryReceived { get; set; }
    public int PaymentCount { get; set; }
    public DateTime FirstPaymentDate { get; set; }
    public DateTime LastPaymentDate { get; set; }
    public decimal AverageMonthlySalary { get; set; }
    public decimal LastSalaryAmount { get; set; }
    public bool HasOutstandingLoans { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public int OutstandingLoanCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<SalaryPaymentViewDto> SalaryPayments { get; set; } = new();
    public List<RemitaLoanViewDto> OutstandingLoans { get; set; } = new();
}

/// <summary>
/// DTO for individual salary payment details
/// </summary>
public class SalaryPaymentViewDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentReference { get; set; } = string.Empty;
    public string Narration { get; set; } = string.Empty;
}

/// <summary>
/// DTO for outstanding loan details
/// </summary>
public class RemitaLoanViewDto
{
    public decimal LoanAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal MonthlyDeduction { get; set; }
    public DateTime LoanDate { get; set; }
    public string LoanReference { get; set; } = string.Empty;
    public string LenderName { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for filtering salary history
/// </summary>
public class SalaryHistoryFilterRequestDto
{
    public string? BorrowerEmail { get; set; }
    public string? BVN { get; set; }
    public string? EmployerName { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinSalaryAmount { get; set; }
    public decimal? MaxSalaryAmount { get; set; }
    public bool? HasOutstandingLoans { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "desc"; // asc or desc
}

/// <summary>
/// Response DTO for paginated salary history results
/// </summary>
public class PaginatedSalaryHistoryResponseDto
{
    public List<SalaryHistoryViewDto> Data { get; set; } = new();
    public int TotalRecords { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
