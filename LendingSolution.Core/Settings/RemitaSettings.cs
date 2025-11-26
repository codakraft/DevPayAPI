namespace LendingSolution.Core.Settings;

public class RemitaSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AuthUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string MerchantId { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public string ServiceTypeId { get; set; } = string.Empty;
    public string MandateUrl { get; set; } = string.Empty;
      // When true the service will contact the live Remita API. When false it will return mocked data.
      // Default to false so services run in mock mode when the setting is not present in configuration.
      public bool UseLiveData { get; set; } = false;
}
