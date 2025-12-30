namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO representing the Remita create mandate response
/// </summary>
public class RemitaCreateMandateResponseDto
{
    public string Status { get; set; } = string.Empty;
    public bool HasData { get; set; }
    public string ResponseId { get; set; } = string.Empty;
    public string ResponseDate { get; set; } = string.Empty;
    public string? RequestDate { get; set; }
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseMsg { get; set; } = string.Empty;
    public RemitaCreateMandateDataDto? Data { get; set; }
}

/// <summary>
/// DTO for the data section of create mandate response
/// </summary>
public class RemitaCreateMandateDataDto
{
    public string AuthorisationCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string MandateReference { get; set; } = string.Empty;
}
