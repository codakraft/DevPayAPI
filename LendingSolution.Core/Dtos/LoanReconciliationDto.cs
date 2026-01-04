namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO for loan collection reconciliation response
/// </summary>
public class LoanReconciliationResponseDto
{
    public Guid LoanId { get; set; }
    public string MandateRef { get; set; } = string.Empty;
    public bool IsReconciled { get; set; }
    public decimal RemitaTotalCollected { get; set; }
    public decimal LocalTotalRepaid { get; set; }
    public decimal Discrepancy { get; set; }
    public int RemitaPaymentCount { get; set; }
    public int LocalPaymentCount { get; set; }
    public decimal TotalDue { get; set; }
    public decimal AmountUnpaid { get; set; }
    public string RepaymentStatus { get; set; } = string.Empty;
    public string LoanStatus { get; set; } = string.Empty;
    public DateTime ReconciledAt { get; set; }
    public string? ReconciledBy { get; set; }
    public string Message { get; set; } = string.Empty;
}
