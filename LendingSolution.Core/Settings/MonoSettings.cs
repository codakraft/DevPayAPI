namespace LendingSolution.Core.Settings;

public class MonoSettings
{
    public string BaseUrl { get; set; } = "https://api.withmono.com";
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string MonoCreditHistoryBVN { get; set; } = string.Empty;
    public string CreditHistoryProvider { get; set; } = "xds";
    public string TestCustomerId { get; set; } = string.Empty;
}