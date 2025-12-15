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
    public string Purpose { get; set; } = "Personal Loan"; // Default purpose
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
    public string? ProcessingReason { get; set; } // Reason for approval/rejection
    public DateTime? MandateCreatedAt { get; set; }
    public Guid CompanyId { get; set; }
    
    [JsonIgnore]
    public Company Company { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    
    [JsonIgnore]
    public LoanProduct Product { get; set; } = default!;
    public bool IsMandateGenerated { get; set; } = false;
    public string MandateId { get; set; } = string.Empty;
    public string RemitaTransRef { get; set; } = string.Empty;
    
    // Navigation property to BorrowerApplication
    public BorrowerApplication? BorrowerApplication { get; set; }
    
    // Additional properties for Remita integration
    public DateTime? DisbursementDate { get; set; }
    public string? DisbursementReference { get; set; }
    public string? MandateStatus { get; set; }
    public DateTime? MandateActivationDate { get; set; }
    public DateTime? MandateStoppedDate { get; set; }
    public string? FailureReason { get; set; }
    
    // Offer Letter properties
    public Guid? OfferLetterDocumentId { get; set; }
    public string? OfferLetterUrl { get; set; }
    public DateTime? OfferLetterSentAt { get; set; }
    public string? SignedOfferLetterUrl { get; set; }
    public DateTime? SignedOfferLetterUploadedAt { get; set; }
}