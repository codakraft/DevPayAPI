using LendingSolution.Core.Models;

namespace LendingSolution.Core.Models;

public class Disbursement : Base
{
    public required string LoanId { get; set; }
    // public Loan Loan { get; set; } = default!; // Temporarily commented out due to type mismatch
    
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Disbursed, Failed
    public string AccountDetails { get; set; } = string.Empty;
    public string DisbursementMethod { get; set; } = "BankTransfer"; // BankTransfer, Wallet, etc.
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
}
