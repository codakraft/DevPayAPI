namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO representing the Remita stop mandate response
/// </summary>
public class RemitaStopMandateResponseDto
{
    public string Status { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public string ResponseId { get; set; } = string.Empty;
    public string ResponseDate { get; set; } = string.Empty;
    public string? RequestDate { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMsg { get; set; } = string.Empty;
    public RemitaStopMandateDataDto? Data { get; set; }
}

/// <summary>
/// DTO for the data section of stop mandate response
/// </summary>
public class RemitaStopMandateDataDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MandateReference { get; set; } = string.Empty;
}
