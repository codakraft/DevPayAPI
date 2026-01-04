namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO representing the Remita salary history response
/// </summary>
public class RemitaSalaryHistoryResponseDto
{
    public string Status { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public string ResponseId { get; set; } = string.Empty;
    public string ResponseDate { get; set; } = string.Empty;
    public string RequestDate { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMsg { get; set; } = string.Empty;
    public RemitaSalaryDataDto? Data { get; set; }
    
    /// <summary>
    /// The authorization code that was used for this request (not from Remita response, but passed through)
    /// </summary>
    public string? AuthorisationCode { get; set; }
}

/// <summary>
/// DTO for the data section of Remita response
/// </summary>
public class RemitaSalaryDataDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string? BVN { get; set; }
    public string? CompanyName { get; set; }
    public string? CustomerName { get; set; }
    public string? Category { get; set; }
    public string? FirstPaymentDate { get; set; }
    public string SalaryCount { get; set; } = "0";
    public List<RemitaSalaryPaymentDto> SalaryPaymentDetails { get; set; } = new();
    public List<RemitaLoanHistoryDto> LoanHistoryDetails { get; set; } = new();
    public string? OriginalCustomerId { get; set; }
}

/// <summary>
/// DTO for individual salary payment
/// </summary>
public class RemitaSalaryPaymentDto
{
    public string PaymentDate { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
}

/// <summary>
/// DTO for loan history details
/// </summary>
public class RemitaLoanHistoryDto
{
    public string? LoanProvider { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string? LoanDisbursementDate { get; set; }
    public string? Status { get; set; }
    public decimal RepaymentAmount { get; set; }
    public string? RepaymentFreq { get; set; }
}

/// <summary>
/// DTO for salary eligibility calculation result
/// </summary>
public class SalaryEligibilityDto
{
    public decimal AverageMonthlySalary { get; set; }
    public decimal LatestSalaryAmount { get; set; }
    public int ConsistentMonths { get; set; }
    public bool HasOutstandingLoans { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public decimal CalculatedMinEligible { get; set; }
    public decimal CalculatedMaxEligible { get; set; }
    public decimal FinalMinEligible { get; set; }
    public decimal FinalMaxEligible { get; set; }
    public string EligibilityReason { get; set; } = string.Empty;
}
