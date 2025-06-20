using Lending.Core.Models;

namespace LendingSolution.Core.Models;

public class LoanProduct : Base
{
    public required Guid CompanyId { get; set; }
    public Company Company { get; set; } =  default!;
    public required string Name { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenure { get; set; } = 0;
    public int MaxTenure { get; set; } = 0;
    public bool IsActive { get; set; } = false;
    public int Moratorium { get; set; } = 0;
}