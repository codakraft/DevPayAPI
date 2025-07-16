namespace LendingSolution.Core.Settings;

/// <summary>
/// Paystack configuration settings
/// </summary>
public class PaystackSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.paystack.co";
    public string CallbackUrl { get; set; } = string.Empty;
}
