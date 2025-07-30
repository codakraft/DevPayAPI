namespace LendingSolution.Core.Dtos.Response.Remita;

public class StopMandateResponseDto : ResponseBase
{
    public string MandateId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public DateTime StoppedDate { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMessage { get; set; } = string.Empty;
}
