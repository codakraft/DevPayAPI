namespace LendingSolution.Core.Models;

public class MonoMandateReference
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid LoanId { get; set; }
    public string MandateId { get; set; } = string.Empty;
    public string? MonoUrl { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string NibssCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MandateType { get; set; } = string.Empty;
    public string DebitType { get; set; } = string.Empty;
    public bool ReadyToDebit { get; set; }
    public bool Approved { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Bank { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string FeeBearer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool LiveMode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime InitialDebitDate { get; set; }
    public int Amount { get; set; }
    public int InitialDebitAmount { get; set; }
    public string TransferDestinationsJson { get; set; } = string.Empty; // JSON string for transfer destinations
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Company? Company { get; set; }
    public Loan? Loan { get; set; }
}