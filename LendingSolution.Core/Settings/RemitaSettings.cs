namespace LendingSolution.Core.Settings;

public class RemitaSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AuthUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string AuthorizationCode { get; set; } = string.Empty;
    public string ServiceTypeId { get; set; } = string.Empty;
    public string MandateUrl { get; set; } = string.Empty;
    public string SalaryHistoryEndpoint { get; set; } = string.Empty;
    public string CreateMandateEndpoint { get; set; } = string.Empty;
    public string StopMandateEndpoint { get; set; } = string.Empty;
    public string MandateHistoryEndpoint { get; set; } = string.Empty;
    
    // Connect Gateway settings for mandate activation via payment
    public string ConnectGatewayBaseUrl { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    
    // Direct Debit mandate endpoints (echannelsvc/echannel/mandate/)
    public string GenerateMandateEndpoint { get; set; } = string.Empty;
    public string ActivateMandateOtpEndpoint { get; set; } = string.Empty;
    public string ValidateMandateOtpEndpoint { get; set; } = string.Empty;
    public string StopDirectDebitMandateEndpoint { get; set; } = string.Empty;

    // When true the service will contact the live Remita API. When false it will use mock data.
    // Default to true for backward compatibility
    public bool UseLiveData { get; set; } = true;
}
