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

// Step 2: Bank and BVN information
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
    public string Message { get; set; } = "BVN validated successfully";
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
    public string Message { get; set; } = "BVN validation successful";
}

// Step 3: Address and documents
public class BorrowerStep3RequestDto
{
    [Required]
    public required string Address { get; set; }
    
    [Required]
    public required string IdNumber { get; set; }
    
    [Required]
    public required string FrontImageBase64 { get; set; }

    [Required]
    public required string FrontImageExtension { get; set; } = "png"; // Default to PNG

    
    [Required]
    public required string BackImageBase64 { get; set; }
    
    [Required]
    public required string BackImageExtension { get; set; } = "png"; // Default to PNG

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

// Step 4: Loan application
public class BorrowerStep4RequestDto
{
    [Required]
    public decimal LoanAmount { get; set; }
    
    [Required]
    public int Tenor { get; set; }
    
    [Required]
    public Guid LoanId { get; set; }
}

public class BorrowerStep4ResponseDto
{
    public decimal RepaymentAmount { get; set; }
    public int Tenor { get; set; }
    public decimal MonthlyRepaymentAmount { get; set; }
    public string Message { get; set; } = "Loan application submitted successfully";
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

// BVN OTP endpoints
public class GenerateBvnOtpRequestDto
{
    [Required]
    public required string BVN { get; set; }
}

public class GenerateBvnOtpResponseDto
{
    public string Message { get; set; } = "BVN OTP sent successfully";
}

public class ValidateBvnOtpRequestDto
{
    [Required]
    public required string BVN { get; set; }
    
    [Required]
    public required string Otp { get; set; }
}

public class ValidateBvnOtpResponseDto
{
    public string Message { get; set; } = "BVN validation successful";
}
