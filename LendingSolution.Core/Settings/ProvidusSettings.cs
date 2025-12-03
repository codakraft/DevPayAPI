namespace LendingSolution.Core.Settings;

/// <summary>
/// Configuration settings for Providus Bank API integration
/// </summary>
public class ProvidusSettings
{
    /// <summary>
    /// The base URL for the Providus API
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// The endpoint path for fund transfers
    /// </summary>
    public string FundTransferEndpoint { get; set; } = "/postingrest/ProvidusFundTransfer";
    
    /// <summary>
    /// Username for API authentication
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for API authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// The debit account (source account) for disbursements
    /// </summary>
    public string DebitAccount { get; set; } = string.Empty;
    
    /// <summary>
    /// Default currency code
    /// </summary>
    public string CurrencyCode { get; set; } = "NGN";
    
    /// <summary>
    /// When true, uses mock responses instead of calling the actual API
    /// Useful for development and testing when no UAT environment is available
    /// </summary>
    public bool UseMockMode { get; set; } = true;
    
    /// <summary>
    /// Timeout in seconds for API calls
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
