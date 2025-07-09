namespace LendingSolution.Core.Dtos.Response.Remita;

public class TransactionStatusResponseDto : ResponseBase
{
    public string TransactionRef { get; set; } = string.Empty;
    public string RemitaTransRef { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string? MandateId { get; set; }
    public string? PayerAccount { get; set; }
    public string? PayerBankCode { get; set; }
    public string? PayerName { get; set; }
    public string? BeneficiaryAccount { get; set; }
    public string? BeneficiaryBankCode { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? TransactionType { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal? Fees { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? FailureReason { get; set; }
}
