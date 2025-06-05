namespace LendingSolution.Core.Dtos;

public class LoanApplicationDto
{
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public required string Purpose { get; set; }
}