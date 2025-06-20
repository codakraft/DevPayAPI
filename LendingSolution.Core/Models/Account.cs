using Lending.Core.Models;

namespace LendingSolution.Core.Models;

public class Account : Base
{
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = default!;
    public required string BankCode { get; set; }
    public required string AccountNumber { get; set; }
    public required string AccountName { get; set; }
    public required string Bvn { get; set; }
    public required decimal MonthlySalary { get; set; }
}
