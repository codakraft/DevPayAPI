using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Models;

public enum BorrowerOnboardingStep
{
    Step1_EmailSent = 1,
    Step1B_EmailValidated = 2,
    Step2_BvnSent = 3,
    Step2B_BvnValidated = 4,
    Step3_DocumentsUploaded = 5,
    Step4_LoanSubmitted = 6
}

public class BorrowerApplication : Base
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Employer { get; set; }
    public string? PhoneNumber { get; set; }
    
    // Bank information
    public string? BankCode { get; set; }
    public string? AccountNo { get; set; }
    public string? BVN { get; set; }
    
    // Address and documents (Step 2)
    public string? Address { get; set; }
    public string? IdNumber { get; set; }
    public string? DocumentIds { get; set; } // Stores comma-separated document IDs
    
    // Loan information
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    
    public Guid ProductId { get; set; }
    public LoanProduct Product { get; set; } = default!;
    
    public Guid? LoanId { get; set; }
    public Loan? Loan { get; set; }
    
    // Eligibility (calculated in Step 2)
    public decimal? MaxLoanEligible { get; set; }
    public decimal? MinLoanEligible { get; set; }
    public int? MaxTenor { get; set; }
    public int? MinTenor { get; set; }
    
    // Tracking
    public BorrowerOnboardingStep CurrentStep { get; set; } = BorrowerOnboardingStep.Step1_EmailSent;
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? BvnVerifiedAt { get; set; }
    public DateTime? DocumentsUploadedAt { get; set; }
    public DateTime? LoanSubmittedAt { get; set; }
    
    // OTP tracking
    public string? LastEmailOtp { get; set; }
    public DateTime? EmailOtpGeneratedAt { get; set; }
    public string? LastBvnOtp { get; set; }
    public DateTime? BvnOtpGeneratedAt { get; set; }
    
    // Mono BVN Verification
    public string? MonoBvnSessionId { get; set; }
    public string? MonoBvnMethod { get; set; } // Selected method: phone, phone_1, email, alternate_phone
    public string? MonoBvnMethodHint { get; set; } // Hint message for where OTP will be sent
    public bool IsBvnVerified { get; set; } = false; // Track if BVN verification is complete
    public string? MonoBvnVerifiedData { get; set; } // JSON of verified BVN data from Mono
    
    // Offer Letter Acceptance
    public bool IsOfferLetterAccepted { get; set; } = false;
    public DateTime? OfferLetterAcceptedAt { get; set; }
    
    // Status
    public bool IsCompleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    // Remita Salary History navigation property
    public RemitaSalaryHistory? RemitaSalaryHistory { get; set; }
}
