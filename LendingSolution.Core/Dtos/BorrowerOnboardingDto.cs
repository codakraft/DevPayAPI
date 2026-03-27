using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

// Step 1: Initial borrower information
public class BorrowerStep1RequestDto
{
    [Required]
    public required string Employer { get; set; }

    [Required]
    public required string FirstName { get; set; }

    [Required]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [Phone]
    public required string PhoneNumber { get; set; }

    [Required]
    public Guid ProductId { get; set; }
}

public class BorrowerStep1ResponseDto
{
    public Guid LoanId { get; set; }
    public string Message { get; set; } = "Email OTP sent successfully";
}

// Step 1B: OTP validation
public class BorrowerStep1BRequestDto
{
    [Required]
    public required string Otp { get; set; }

    [Required]
    public Guid LoanId { get; set; }
}

public class BorrowerStep1BResponseDto
{
    public string Message { get; set; } = "Email validated successfully";
}

// Resend Step 1 email OTP
public class ResendStep1EmailOtpRequestDto
{
    [Required]
    public Guid LoanId { get; set; }
}

public class ResendStep1EmailOtpResponseDto
{
    public string Message { get; set; } = "Email OTP has been resent successfully";
}

// Step 2: BVN and Bank details submission
public class BorrowerStep2RequestDto
{
    [Required]
    public required string BankCode { get; set; }

    [Required]
    public required string AccountNo { get; set; }

    [Required]
    public required string BVN { get; set; }

    [Required]
    public Guid LoanId { get; set; }
}

public class BorrowerStep2ResponseDto
{
    public string Message { get; set; } = "BVN verification initiated successfully";
    public string? OtpHint { get; set; } // Hint for where OTP will be sent
}

// Step 2B: BVN OTP validation
public class BorrowerStep2BRequestDto
{
    [Required]
    public required string Otp { get; set; }

    [Required]
    public Guid LoanId { get; set; }
}

public class BorrowerStep2BResponseDto
{
    public string Message { get; set; } = "BVN validated successfully";
}

// Resend Step 2 BVN OTP
public class ResendStep2BvnOtpRequestDto
{
    [Required]
    public Guid LoanId { get; set; }
}

public class ResendStep2BvnOtpResponseDto
{
    public string Message { get; set; } = "BVN OTP has been resent successfully";
}

// Step 3: Address and documents (bank details already collected in Step 2)
public class BorrowerStep3RequestDto
{
    [Required]
    public required string Address { get; set; }

    [Required]
    public required string IdNumber { get; set; }

    [Required]
    public required List<string> ImageIds { get; set; }

    [Required]
    public Guid LoanId { get; set; }
}

public class BorrowerStep3ResponseDto
{
    public decimal MaxLoanEligible { get; set; }
    public decimal MinLoanEligible { get; set; }
    public int MaxTenor { get; set; }
    public int MinTenor { get; set; }
    public string Message { get; set; } = "Information added successfully";
}

public class UpdateDocumentsRequestDto
{
    [Required]
    public required List<string> ImageIds { get; set; }

    [Required]
    public Guid LoanId { get; set; }
}

public class UpdateDocumentsResponseDto
{
    public string Message { get; set; } = "Documents updated successfully";
}

public class RequestImageReuploadDto
{
    [Required]
    public Guid LoanId { get; set; }

    [Required]
    public required string Reason { get; set; }
}

public class RequestImageReuploadResponseDto
{
    public string Message { get; set; } = "Image re-upload request sent successfully";
}

// Step 4: Loan application
public class BorrowerStep4RequestDto
{
    [Required]
    public decimal LoanAmount { get; set; }

    [Required]
    public int Tenor { get; set; }

    [Required]
    public Guid LoanId { get; set; }

    [Required]
    public bool AcceptOfferLetter { get; set; }
}

public class BorrowerStep4ResponseDto
{
    public decimal LoanPrincipal { get; set; } // Loan Principal (LP)
    public decimal ApplicableFees { get; set; } // Applicable Fees (AF)
    public decimal DisbursementAmount { get; set; } // Amount to Disburse (AtD) = LP - AF
    public decimal AppliedInterest { get; set; } // Applied Interest (AI)
    public decimal RepaymentAmount { get; set; } // Total Repayment Amount (RA) = LP + AI
    public int Tenor { get; set; }
    public decimal MonthlyRepaymentAmount { get; set; }
    public string Message { get; set; } = "Loan application submitted successfully";
}

// ─────────────────────────────────────────────────────────────────────────────
// Step 5: Generate Direct Debit Mandate
// ─────────────────────────────────────────────────────────────────────────────

public class BorrowerStep5RequestDto
{
    [Required]
    public Guid LoanId { get; set; }
}

public class BorrowerStep5ResponseDto
{
    public string MandateId { get; set; } = string.Empty;
    public string Message { get; set; } = "Mandate generated successfully. Please proceed to activate the mandate.";
}

// ─────────────────────────────────────────────────────────────────────────────
// Step 6: Request OTP to activate the mandate
// ─────────────────────────────────────────────────────────────────────────────

public class BorrowerStep6RequestDto
{
    [Required]
    public Guid LoanId { get; set; }
}

/// <summary>
/// Describes an input the borrower must supply (e.g. OTP, last 4 digits of card).
/// Mirrors Remita's authParams array entries.
/// </summary>
public class MandateAuthParamDto
{
    public string? Param1 { get; set; }
    public string? Label1 { get; set; }
    public string? Description1 { get; set; }
    public string? Param2 { get; set; }
    public string? Label2 { get; set; }
    public string? Description2 { get; set; }
}

public class BorrowerStep6ResponseDto
{
    /// <summary>The transaction reference to be echoed back in Step 6B.</summary>
    public string RemitaTransRef { get; set; } = string.Empty;
    /// <summary>Auth params from Remita that describe what the borrower must input.</summary>
    public List<MandateAuthParamDto> AuthParams { get; set; } = new();
    public string Message { get; set; } = "OTP sent. Please enter the required details to activate your mandate.";
}

// ─────────────────────────────────────────────────────────────────────────────
// Step 6B: Validate OTP to activate the mandate
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A single auth param value submitted by the borrower (e.g. OTP or card digits).
/// </summary>
public class MandateAuthParamValueDto
{
    /// <summary>Param key from the authParams descriptor (e.g. "OTP").</summary>
    public string? Param1 { get; set; }
    /// <summary>Param key from the authParams descriptor (e.g. "CARD").</summary>
    public string? Param2 { get; set; }
    /// <summary>Borrower's input value for this param.</summary>
    [Required]
    public string Value { get; set; } = string.Empty;
}

public class BorrowerStep6BRequestDto
{
    [Required]
    public Guid LoanId { get; set; }

    /// <summary>
    /// The transaction reference returned in Step 6 (mirrors Remita's remitaTransRef).
    /// </summary>
    [Required]
    public string RemitaTransRef { get; set; } = string.Empty;

    /// <summary>
    /// The borrower's inputs matching the authParams from Step 6
    /// (e.g. OTP value and last 4 digits of bank card).
    /// </summary>
    [Required]
    public List<MandateAuthParamValueDto> AuthParams { get; set; } = new();
}

public class BorrowerStep6BResponseDto
{
    public string MandateId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = "Mandate activated successfully. Your loan application is complete.";
}

// Email OTP endpoints
public class GenerateEmailOtpRequestDto
{
    [Required]
    [EmailAddress]
    public required string EmailAddress { get; set; }
}

public class GenerateEmailOtpResponseDto
{
    public string Message { get; set; } = "OTP sent successfully";
}

public class ValidateEmailOtpRequestDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Otp { get; set; }
}

public class ValidateEmailOtpResponseDto
{
    public string Message { get; set; } = "OTP validation successful";
}

// BVN OTP generation endpoint
public class GenerateBvnOtpRequestDto
{
    [Required]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "BVN must be exactly 11 digits")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "BVN must contain only digits")]
    public required string BVN { get; set; }
}

public class GenerateBvnOtpResponseDto
{
    public string Message { get; set; } = "Verification email has been sent";
}
