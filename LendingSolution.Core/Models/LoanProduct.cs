using LendingSolution.Core.Models;

namespace LendingSolution.Core.Models;

public class LoanProduct : Base
{
    public required Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    public required string Name { get; set; }
    public required string Description { get; set; }

    public required string ShortName { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    public int Moratorium { get; set; } = 30;
    public ICollection<Loan> Loans { get; set; } = [];
}