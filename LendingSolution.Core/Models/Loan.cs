using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Models;

public class Loan : Base
{
    public string? UserId { get; set; } // Optional - can be derived from BorrowerApplication
    public ApplicationUser? User { get; set; }
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public string Purpose { get; set; } = "Personal Loan"; // Default purpose
    public LoanStatus Status { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? MandateCreatedAt { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
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
    public string? FailureReason { get; set; }
}