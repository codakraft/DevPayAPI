namespace LendingSolution.Core.Settings;

/// <summary>
/// SMS configuration settings for SMS providers (Twilio, Termii, etc.)
/// </summary>
public class SmsSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string AccountSid { get; set; } = string.Empty; // For Twilio
    public string AuthToken { get; set; } = string.Empty; // For Twilio
    public string SenderId { get; set; } = "LendingSolution";
    public string BaseUrl { get; set; } = string.Empty;
    public string Provider { get; set; } = "Twilio"; // Twilio, Termii, etc.
}
