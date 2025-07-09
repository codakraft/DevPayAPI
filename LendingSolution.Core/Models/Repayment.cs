using LendingSolution.Core.Models;

namespace LendingSolution.Core.Models;

public class Repayment : Base
{
    public required string LoanId { get; set; }
    // public Loan Loan { get; set; } = default!; // Temporarily commented out due to type mismatch
    
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "BankTransfer";
    public string Status { get; set; } = "Pending"; // Pending, Verified, Failed
    public string? PaymentReference { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    
    // Additional properties for Remita integration
    public DateTime? RepaymentDate { get; set; }
    public string? TransactionReference { get; set; }
}