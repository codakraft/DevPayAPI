using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using System.Text.Json.Serialization;

namespace LendingSolution.Core.Models;

public class Loan : Base
{
    public string? UserId { get; set; } // Optional - can be derived from BorrowerApplication
    
    [JsonIgnore]
    public ApplicationUser? User { get; set; }
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public string Purpose { get; set; } = "Salary Loan"; // Default purpose
    public LoanStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? ApprovedBy { get; set; } // User ID who approved
    
    [JsonIgnore]
    public ApplicationUser? ApprovedByUser { get; set; } // Navigation property
    public string? RejectedBy { get; set; } // User ID who rejected
    
    [JsonIgnore]
    public ApplicationUser? RejectedByUser { get; set; } // Navigation property
    public string? Reason { get; set; } // Reason for approval/rejection/failure
    public DateTime? MandateCreatedAt { get; set; }
    public Guid CompanyId { get; set; }
    
    [JsonIgnore]
    public Company Company { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    
    [JsonIgnore]
    public LoanProduct Product { get; set; } = default!;
    public bool IsMandateCreated { get; set; } = false;
    public string MandateRef { get; set; } = string.Empty;
    
    // Navigation property to BorrowerApplication
    public BorrowerApplication? BorrowerApplication { get; set; }
    
    // Additional properties for Remita integration
    public DateTime? DisbursementDate { get; set; }
    public string? DisbursementReference { get; set; }

    /// <summary>
    /// The transaction reference sent to Providus, persisted before the transfer is
    /// attempted and reused on every retry. Providus deduplicates on this value, so a
    /// stable reference is what prevents a retry after a timeout from paying twice.
    /// </summary>
    public string? DisbursementTransactionRef { get; set; }

    /// <summary>
    /// Set when a disbursement attempt returned an unknown outcome (timeout, connection
    /// failure). The loan must not be re-disbursed while this is true — requery
    /// DisbursementTransactionRef with Providus to establish what actually happened.
    /// </summary>
    public bool DisbursementOutcomeUnknown { get; set; }
    public DateTime? MandateStoppedDate { get; set; }
    public DateTime? MandateStoppedAt { get; set; }
    public string? MandateStoppedBy { get; set; }
 
    // Offer Letter properties
    public Guid? OfferLetterDocumentId { get; set; }
    public string? OfferLetterUrl { get; set; }
    public DateTime? OfferLetterSentAt { get; set; }
    public Guid? SignedOfferLetterDocumentId { get; set; }
    public DateTime? SignedOfferLetterUploadedAt { get; set; }
    
    // Repayment calculation properties
    public decimal? TotalRepayment { get; set; }
    public decimal? MonthlyRepayment { get; set; }
    
    // Disbursement calculation properties
    public decimal? DisbursementAmount { get; set; } // Amount to Disburse = Principal - Applicable Fees
    public decimal? ApplicableFees { get; set; } // Total fees deducted from principal
    public decimal? AppliedInterest { get; set; } // Interest added to principal for repayment
}