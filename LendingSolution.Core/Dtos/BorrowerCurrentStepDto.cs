using LendingSolution.Core.Models;

namespace LendingSolution.Core.Dtos;

public class BorrowerCurrentStepRequestDto
{
    public string Email { get; set; } = string.Empty;
}

public class BorrowerCurrentStepResponseDto
{
    /// <summary>
    /// Contains the BorrowerApplication ID (not the actual Loan ID)
    /// This is used for tracking and referencing the borrower application
    /// </summary>
    public Guid LoanId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public BorrowerOnboardingStep CurrentStep { get; set; }
    public string CurrentStepName { get; set; } = string.Empty;
    public string CurrentStepDescription { get; set; } = string.Empty;
    public string NextStepName { get; set; } = string.Empty;
    public string NextStepDescription { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Step completion timestamps
    public DateTime? EmailVerifiedAt { get; set; }
    public DateTime? BvnVerifiedAt { get; set; }
    public DateTime? DocumentsUploadedAt { get; set; }
    public DateTime? LoanSubmittedAt { get; set; }
    
    // Progress information
    public int StepNumber { get; set; }
    public int TotalSteps { get; set; } = 6;
    public decimal ProgressPercentage { get; set; }
    
    // Eligibility information (available after Step 3)
    public decimal? MaxLoanEligible { get; set; }
    public decimal? MinLoanEligible { get; set; }
    public int? MaxTenor { get; set; }
    public int? MinTenor { get; set; }
    
    // Company and Product information
    public string CompanyName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    
    // Required actions (if any)
    public List<string> RequiredActions { get; set; } = new();
}
