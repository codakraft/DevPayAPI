using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Represents a wallet for tracking financial transactions
/// </summary>
public class Wallet
{
    [Key]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Company that owns this wallet (null for SuperAdmin wallet)
    /// </summary>
    public Guid? CompanyId { get; set; }
    
    /// <summary>
    /// Current balance in the wallet
    /// </summary>
    public decimal Balance { get; set; }
    
    /// <summary>
    /// Total amount that has been credited to this wallet
    /// </summary>
    public decimal TotalCredits { get; set; }
    
    /// <summary>
    /// Total amount that has been debited from this wallet
    /// </summary>
    public decimal TotalDebits { get; set; }
    
    /// <summary>
    /// Indicates if this is the SuperAdmin wallet
    /// </summary>
    public bool IsSuperAdminWallet { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Company? Company { get; set; }
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}

/// <summary>
/// Represents a transaction in a wallet
/// </summary>
public class WalletTransaction
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid WalletId { get; set; }
    
    /// <summary>
    /// Transaction amount (positive for credits, negative for debits)
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Balance after this transaction
    /// </summary>
    public decimal BalanceAfter { get; set; }
    
    /// <summary>
    /// Type of transaction
    /// </summary>
    public WalletTransactionType TransactionType { get; set; }
    
    /// <summary>
    /// Description of the transaction
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Reference ID for the transaction (e.g., loan ID, payment reference)
    /// </summary>
    public string? ReferenceId { get; set; }
    
    /// <summary>
    /// User who initiated the transaction
    /// </summary>
    public string? InitiatedBy { get; set; }
    
    /// <summary>
    /// Paystack payment reference (if applicable)
    /// </summary>
    public string? PaystackReference { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Wallet Wallet { get; set; } = null!;
    public ApplicationUser? InitiatedByUser { get; set; }
}

/// <summary>
/// Types of wallet transactions
/// </summary>
public enum WalletTransactionType
{
    Credit = 1,
    Debit = 2,
    FeeCharge = 3,
    Refund = 4,
    PaystackFunding = 5,
    LoanDisbursementFee = 6,
    ProcessingFee = 7,
    ManagementFee = 8,
    LegalFee = 9,
    OtpFee = 10
}
