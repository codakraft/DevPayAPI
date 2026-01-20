using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO for offer letter details shown to borrower
/// </summary>
public class OfferLetterDto
{
    public Guid LoanId { get; set; }
    public string BorrowerName { get; set; } = string.Empty;
    public string BorrowerEmail { get; set; } = string.Empty;
    public string BorrowerAddress { get; set; } = string.Empty;
    
    // Company details
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    
    // Loan details
    public decimal LoanAmount { get; set; }
    public int DurationInMonths { get; set; }
    public decimal InterestRate { get; set; }
    public string InterestComputationBasis { get; set; } = string.Empty;
    public decimal TotalInterest { get; set; }
    public decimal TotalRepayment { get; set; }
    public decimal MonthlyRepayment { get; set; }
    public string Purpose { get; set; } = string.Empty;
    
    // Fees
    public decimal ProcessingFee { get; set; }
    public decimal ManagementFee { get; set; }
    public decimal TotalFees { get; set; }
    public decimal DisbursementAmount { get; set; } // Amount to be disbursed (LoanAmount - TotalFees)
    
    // Product details
    public string ProductName { get; set; } = string.Empty;
    public decimal PenaltyRate { get; set; }
    public int MoratoriumDays { get; set; }
    
    // Dates
    public DateTime OfferDate { get; set; }
    public DateTime ExpiryDate { get; set; } // Offer validity period
    public DateTime ExpectedDisbursementDate { get; set; }
    public DateTime ExpectedMaturityDate { get; set; }
    
    // Document URLs
    public string? OfferLetterUrl { get; set; }
}

/// <summary>
/// DTO for uploading signed offer letter
/// </summary>
public class SignedOfferLetterUploadDto
{
    [Required(ErrorMessage = "Signed offer letter document ID is required")]
    public string SignedOfferLetterDocumentId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for offer letter response after sending
/// </summary>
public class OfferLetterResponseDto
{
    public Guid LoanId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? OfferLetterUrl { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// DTO for signed offer letter upload response
/// </summary>
public class SignedOfferLetterResponseDto
{
    public Guid LoanId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SignedOfferLetterDocumentId { get; set; }
    public DateTime? UploadedAt { get; set; }
    public bool ReadyForDisbursement { get; set; }
}

/// <summary>
/// DTO for loan disbursement response
/// </summary>
public class LoanDisbursementResponseDto
{
    public Guid LoanId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? DisbursedAt { get; set; }
    public string? DisbursementReference { get; set; }
    public DateTime? DueDate { get; set; }
}
