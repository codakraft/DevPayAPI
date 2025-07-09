namespace LendingSolution.Core.Dtos.Response.Remita;

public class DisbursementResponseDto : ResponseBase
{
    public string TransactionRef { get; set; } = string.Empty;
    public string RemitaTransRef { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BeneficiaryAccount { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string BeneficiaryBankCode { get; set; } = string.Empty;
    public string BeneficiaryBankName { get; set; } = string.Empty;
    public string DebitAccount { get; set; } = string.Empty;
    public string DebitBankCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Narration { get; set; }
    public string? Reference { get; set; }
    public decimal? Fees { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
}
