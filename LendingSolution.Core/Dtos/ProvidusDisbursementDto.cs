using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

#region Providus Fund Transfer DTOs

/// <summary>
/// Request DTO for Providus fund transfer API
/// </summary>
public class ProvidusFundTransferRequestDto
{
    /// <summary>
    /// The credit (destination) account number
    /// </summary>
    [Required]
    [JsonPropertyName("creditAccount")]
    public string CreditAccount { get; set; } = string.Empty;
    
    /// <summary>
    /// The debit (source) account number
    /// </summary>
    [Required]
    [JsonPropertyName("debitAccount")]
    public string DebitAccount { get; set; } = string.Empty;
    
    /// <summary>
    /// The transaction amount as string (Providus expects string format)
    /// </summary>
    [Required]
    [JsonPropertyName("transactionAmount")]
    public string TransactionAmount { get; set; } = string.Empty;
    
    /// <summary>
    /// Currency code (default: NGN)
    /// </summary>
    [Required]
    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = "NGN";
    
    /// <summary>
    /// Transaction narration/description
    /// </summary>
    [Required]
    [JsonPropertyName("narration")]
    public string Narration { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique transaction reference
    /// </summary>
    [Required]
    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;
    
    /// <summary>
    /// API username for authentication
    /// </summary>
    [Required]
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// API password for authentication
    /// </summary>
    [Required]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO from Providus fund transfer API
/// </summary>
public class ProvidusFundTransferResponseDto
{
    /// <summary>
    /// The transferred amount
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
    
    /// <summary>
    /// Transaction reference from Providus
    /// </summary>
    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Currency code
    /// </summary>
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    /// <summary>
    /// Response message from the API
    /// </summary>
    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// Response code from the API (00 = success)
    /// </summary>
    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if the transaction was successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccessful => ResponseCode == "00";
}

#endregion

#region Internal Disbursement DTOs

/// <summary>
/// Internal request DTO for initiating a Providus disbursement
/// </summary>
public class ProvidusDisbursementInternalRequestDto
{
    /// <summary>
    /// The loan ID being disbursed
    /// </summary>
    [Required]
    public Guid LoanId { get; set; }
    
    /// <summary>
    /// The destination account number
    /// </summary>
    [Required]
    public string DestinationAccountNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// The destination bank code
    /// </summary>
    [Required]
    public string DestinationBankCode { get; set; } = string.Empty;
    
    /// <summary>
    /// The amount to disburse
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Narration for the transaction
    /// </summary>
    public string? Narration { get; set; }
    
    /// <summary>
    /// Beneficiary name for reference
    /// </summary>
    public string? BeneficiaryName { get; set; }
}

/// <summary>
/// Response DTO for disbursement operations
/// </summary>
public class DisbursementResultDto
{
    /// <summary>
    /// Whether the disbursement was successful
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// The loan ID
    /// </summary>
    public Guid LoanId { get; set; }
    
    /// <summary>
    /// The disbursed amount
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Unique disbursement reference
    /// </summary>
    public string DisbursementReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Provider transaction reference (from Providus)
    /// </summary>
    public string ProviderReference { get; set; } = string.Empty;
    
    /// <summary>
    /// Response message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Response code from provider
    /// </summary>
    public string ResponseCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of the disbursement
    /// </summary>
    public DateTime DisbursedAt { get; set; }
    
    /// <summary>
    /// Indicates if this was a mock transaction
    /// </summary>
    public bool IsMockTransaction { get; set; }
    
    /// <summary>
    /// Error details if the transaction failed
    /// </summary>
    public string? ErrorDetails { get; set; }
}

#endregion
