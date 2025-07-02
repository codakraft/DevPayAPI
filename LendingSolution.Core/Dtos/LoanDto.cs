using System.ComponentModel.DataAnnotations;
using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Dtos;

public class LoanDto
{
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public required string Purpose { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid CompanyId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public LoanStatus Status { get; set; }

}


public class CreateLoanRequestDto : LoanDto
{
    public required string UserId { get; set; }
}

public class LoanResponseDto : LoanDto
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? MandateCreatedAt { get; set; }
}

public class InitiateMandateOtpRequestDto
{

    public required string MandateId { get; set; }
    public required string RequestId { get; set; }
}


public class AuthParam
{
    public string? Param1 { get; set; }
    public string? Param2 { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class ValidateMandateOtpRequestDto
{
    public required string OtpCode { get; set; }
}

public class DebitInstructionRequestDto
{
    public string MerchantId { get; set; } = string.Empty;
    public string ServiceTypeId { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string TotalAmount { get; set; } = string.Empty;
    public string MandateId { get; set; } = string.Empty;
    public string FundingAccount { get; set; } = string.Empty;
    public string FundingBankCode { get; set; } = string.Empty;
}


public class InitiateMandateOtpResponseDto { }
public class ValidateMandateOtpResponseDto { }
public class DebitInstructionResponseDto { }