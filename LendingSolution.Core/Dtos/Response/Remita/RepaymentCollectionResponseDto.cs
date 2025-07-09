namespace LendingSolution.Core.Dtos.Response.Remita;

public class RepaymentCollectionResponseDto : ResponseBase
{
    public string TransactionRef { get; set; } = string.Empty;
    public string RemitaTransRef { get; set; } = string.Empty;
    public string MandateId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PayerAccount { get; set; } = string.Empty;
    public string PayerBankCode { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string CollectionStatus { get; set; } = string.Empty;
    public DateTime CollectionDate { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? CollectionType { get; set; }
    public decimal? Fees { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
}
