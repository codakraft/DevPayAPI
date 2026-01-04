using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class DisbursementRequestDto
{
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string AccountDetails { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    [Required]
    public string DisbursementMethod { get; set; } = "BankTransfer"; // BankTransfer, Wallet, etc.
}

public class RepaymentRequestDto
{
    [Required]
    public string LoanId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string PaymentMethod { get; set; } = "BankTransfer";
    
    [MaxLength(500)]
    public string? PaymentReference { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class DisbursementDto
{
    public string Id { get; set; } = string.Empty;
    public string LoanId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AccountDetails { get; set; } = string.Empty;
    public string DisbursementMethod { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
}

public class RepaymentDto
{
    public string Id { get; set; } = string.Empty;
    public Guid LoanId { get; set; }
    public decimal TotalDue { get; set; }
    public decimal TotalRepaid { get; set; }
    public DateTime? LastPaymentAt { get; set; }
    public decimal AmountUnpaid { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }    public string? BorrowerName { get; set; }
    public string? BorrowerEmail { get; set; }
    public string? CompanyName { get; set; }
}

public class RepaymentFilterDto
{
    public string? Search { get; set; } // Borrower name, email, loan ID
    public Guid? CompanyId { get; set; }
    public string? Status { get; set; } // Active, Overdue, Completed
    public decimal? MinTotalDue { get; set; }
    public decimal? MaxTotalDue { get; set; }
    public decimal? MinAmountUnpaid { get; set; }
    public decimal? MaxAmountUnpaid { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public DateTime? LastPaymentAfter { get; set; }
    public DateTime? LastPaymentBefore { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortOrder { get; set; } = "desc";
}

public class PagedRepaymentDto
{
    public List<RepaymentDto> Repayments { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    
    // Summary statistics
    public decimal TotalDueAmount { get; set; }
    public decimal TotalRepaidAmount { get; set; }
    public decimal TotalUnpaidAmount { get; set; }
    public int ActiveCount { get; set; }
    public int OverdueCount { get; set; }
    public int CompletedCount { get; set; }}

public class FinanceReportDto
{
    public decimal TotalDisbursed { get; set; }
    public decimal TotalRepaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int CompletedLoans { get; set; }
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
}

public class CompanyWalletDto
{
    public string CompanyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public decimal PendingDisbursements { get; set; }
    public decimal TotalReceived { get; set; }
    public DateTime LastUpdated { get; set; }
}
