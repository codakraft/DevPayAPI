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
    public string LoanId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
}

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
