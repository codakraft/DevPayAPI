namespace LendingSolution.Core.Dtos;

public class LoanProductDto
{
    public required Guid CompanyId { get; set; }

    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    public int Moratorium { get; set; } = 0;

}

public class CreateLoanProductRequestDto : LoanProductDto;

public class UpdateLoanProductRequestDto
{
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public required string Description { get; set; }
    public decimal InterestRate { get; set; } = 0;
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    public int Moratorium { get; set; } = 0;
}

public class LoanProductResponseDto : LoanProductDto
{
    public Guid Id { get; set; }
}