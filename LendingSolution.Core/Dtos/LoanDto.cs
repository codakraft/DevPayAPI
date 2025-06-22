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
