namespace LendingSolution.Core.Models;

public class Company : Base
{
    public required string Name { get; set; }
    public required string ShortName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? LogoDocumentId { get; set; }
    public ICollection<LoanProduct> LoanProducts { get; set; } = [];
}
