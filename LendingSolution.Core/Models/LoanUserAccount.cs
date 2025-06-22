namespace LendingSolution.Core.Models;

public class LoanUserAccount
{
    public Loan Loan { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}