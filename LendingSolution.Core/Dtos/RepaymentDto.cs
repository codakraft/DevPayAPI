namespace LendingSolution.Core.Dtos;

public class RepaymentDto
{
    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
}