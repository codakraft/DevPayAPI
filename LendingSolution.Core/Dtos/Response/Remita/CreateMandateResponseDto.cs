namespace LendingSolution.Core.Dtos.Response.Remita;

public class CreateMandateResponseDto : ResponseBase
{
    public string MandateId { get; set; } = string.Empty;
    public string RemitaTransRef { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PayerAccount { get; set; } = string.Empty;
    public string PayerBankCode { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string MandateType { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string MandateStatus { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool RequiresOtp { get; set; }
}
