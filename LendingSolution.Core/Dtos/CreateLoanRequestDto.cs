namespace LendingSolution.Core.Dtos;

public class CreateLoanRequestDto : LoanDto
{
    public required string UserId { get; set; }
}