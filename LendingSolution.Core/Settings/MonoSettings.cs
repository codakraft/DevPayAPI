namespace LendingSolution.Core.Settings;

public class MonoSettings
{
    public string BaseUrl { get; set; } = "https://api.withmono.com";
    public string SecretKey { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;

    /// <summary>
    /// URL Mono redirects the borrower to after they complete the e-mandate authorization.
    /// </summary>
    public string MandateRedirectUrl { get; set; } = string.Empty;
    public string MonoCreditHistoryBVN { get; set; } = string.Empty;
    public string CreditHistoryProvider { get; set; } = "xds";
    public string TestCustomerId { get; set; } = string.Empty;

    /// <summary>
    /// How long (in days) a credit-analysis result is cached in MonoCreditAnalysisRecords
    /// before a fresh Mono call is made. Set to 0 (or less) to disable caching entirely.
    /// </summary>
    public int CreditHistoryCacheDays { get; set; } = 30;
}