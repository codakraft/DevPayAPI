namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO representing the Remita mandate history response
/// </summary>
public class RemitaMandateHistoryResponseDto
{
    public string Status { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public string ResponseId { get; set; } = string.Empty;
    public string ResponseDate { get; set; } = string.Empty;
    public string? RequestDate { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMsg { get; set; } = string.Empty;
    public RemitaMandateHistoryDataDto? Data { get; set; }
}

/// <summary>
/// DTO for the data section of mandate history response
/// </summary>
public class RemitaMandateHistoryDataDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string LoanMandateReference { get; set; } = string.Empty;
    public decimal TotalDisbursed { get; set; }
    public decimal OutstandingLoanBal { get; set; }
    public string? LoanRepaymentRef { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public string SalaryAccount { get; set; } = string.Empty;
    public string AuthorisationCode { get; set; } = string.Empty;
    public string SalaryBankCode { get; set; } = string.Empty;
    public string DisbursementAccountBank { get; set; } = string.Empty;
    public string CollectionStartDate { get; set; } = string.Empty;
    public string DateOfDisbursement { get; set; } = string.Empty;
    public string DisbursementAccount { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LenderDetails { get; set; } = string.Empty;
    public object? Repayment { get; set; }
}
