namespace LendingSolution.Core.Dtos;

public class LoanApprovalRequestDto
{
    public string? Reason { get; set; }
}

public class LoanRejectionRequestDto
{
    public string? Reason { get; set; }
}

public class ProcessLoanRequestDto
{
    public string Action { get; set; } = string.Empty; // "approve" or "reject"
    public string? Reason { get; set; }
}

public class LoanApprovalResponseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
}
