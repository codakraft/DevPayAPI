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
    /// The endpoint path for intra-bank (Providus to Providus) fund transfers
    /// </summary>
    public string FundTransferEndpoint { get; set; } = "/postingrest/ProvidusFundTransfer";

    /// <summary>
    /// The endpoint path for inter-bank (NIP) fund transfers
    /// </summary>
    public string NipFundTransferEndpoint { get; set; } = "/postingrest/NIPFundTransfer";

    /// <summary>
    /// The endpoint path for NIP beneficiary name enquiry
    /// </summary>
    public string NipAccountEnquiryEndpoint { get; set; } = "/postingrest/GetNIPAccount";

    /// <summary>
    /// The endpoint path for requerying the status of a NIP transfer
    /// </summary>
    public string NipTransactionStatusEndpoint { get; set; } = "/postingrest/GetNIPTransactionStatus";

    /// <summary>
    /// The endpoint path for the authoritative NIBSS bank list
    /// </summary>
    public string NipBanksEndpoint { get; set; } = "/postingrest/GetNIPBanks";

    /// <summary>
    /// Providus Bank's own NIBSS institution code. Transfers to this bank are routed
    /// through the cheaper intra-bank endpoint rather than over NIP.
    /// </summary>
    public string ProvidusNipBankCode { get; set; } = "000023";

    /// <summary>
    /// The name of the source account, sent as sourceAccountName on NIP transfers.
    /// </summary>
    public string SourceAccountName { get; set; } = string.Empty;

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
