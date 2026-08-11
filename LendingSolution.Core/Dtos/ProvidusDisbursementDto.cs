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

#region Providus NIP (inter-bank) DTOs

/// <summary>
/// Request DTO for the Providus NIPFundTransfer API (inter-bank transfers).
/// Note there is no debitAccount field: the source account is bound to the API
/// credentials server-side, and only its name is echoed via sourceAccountName.
/// </summary>
public class ProvidusNipFundTransferRequestDto
{
    [JsonPropertyName("beneficiaryAccountName")]
    public string BeneficiaryAccountName { get; set; } = string.Empty;

    [JsonPropertyName("beneficiaryAccountNumber")]
    public string BeneficiaryAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// The beneficiary's 6-digit NIBSS institution code (not the 3-digit CBN code)
    /// </summary>
    [JsonPropertyName("beneficiaryBank")]
    public string BeneficiaryBank { get; set; } = string.Empty;

    [JsonPropertyName("transactionAmount")]
    public string TransactionAmount { get; set; } = string.Empty;

    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = "NGN";

    [JsonPropertyName("narration")]
    public string Narration { get; set; } = string.Empty;

    [JsonPropertyName("sourceAccountName")]
    public string SourceAccountName { get; set; } = string.Empty;

    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO from the Providus NIPFundTransfer API
/// </summary>
public class ProvidusNipFundTransferResponseDto
{
    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;

    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsSuccessful => ResponseCode == ProvidusResponseCodes.Success;

    /// <summary>
    /// Providus rejects a reference it has already seen. That is a duplicate guard,
    /// not a failure: the original transfer may well have succeeded.
    /// </summary>
    [JsonIgnore]
    public bool IsDuplicateReference => ResponseCode == ProvidusResponseCodes.TransactionReferenceExists;
}

/// <summary>
/// Request DTO for the Providus GetNIPAccount API (beneficiary name enquiry)
/// </summary>
public class ProvidusNipAccountEnquiryRequestDto
{
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// The beneficiary's 6-digit NIBSS institution code
    /// </summary>
    [JsonPropertyName("beneficiaryBank")]
    public string BeneficiaryBank { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO from the Providus GetNIPAccount API
/// </summary>
public class ProvidusNipAccountEnquiryResponseDto
{
    [JsonPropertyName("accountName")]
    public string AccountName { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;

    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;

    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsSuccessful => ResponseCode == ProvidusResponseCodes.Success;
}

/// <summary>
/// Request DTO for the Providus GetNIPTransactionStatus API
/// </summary>
public class ProvidusNipTransactionStatusRequestDto
{
    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO from the Providus GetNIPTransactionStatus API
/// </summary>
public class ProvidusNipTransactionStatusResponseDto
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("recipientBankCode")]
    public string RecipientBankCode { get; set; } = string.Empty;

    [JsonPropertyName("recipientAccountNumber")]
    public string RecipientAccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("transactionReference")]
    public string TransactionReference { get; set; } = string.Empty;

    [JsonPropertyName("transactionDateTime")]
    public string TransactionDateTime { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("responseMessage")]
    public string ResponseMessage { get; set; } = string.Empty;

    [JsonPropertyName("responseCode")]
    public string ResponseCode { get; set; } = string.Empty;

    [JsonIgnore]
    public bool IsSuccessful => ResponseCode == ProvidusResponseCodes.Success;

    /// <summary>
    /// Providus has no record of this reference, so no funds moved against it.
    /// </summary>
    [JsonIgnore]
    public bool IsNotFound => ResponseCode == ProvidusResponseCodes.TransactionNotFound;
}

/// <summary>
/// Response codes returned by the Providus APIs
/// </summary>
public static class ProvidusResponseCodes
{
    public const string Success = "00";
    public const string TransactionNotFound = "01";
    public const string Unauthorized = "11";
    public const string InvalidCreditAccount = "7701";
    public const string TransactionReferenceExists = "7709";

    /// <summary>Local codes, never returned by Providus.</summary>
    public const string Timeout = "TIMEOUT";
    public const string HttpError = "HTTP_ERROR";
    public const string Error = "ERROR";
    public const string UnresolvedBankCode = "BANK_CODE_UNRESOLVED";
    public const string NameEnquiryFailed = "NAME_ENQUIRY_FAILED";
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

    /// <summary>
    /// Caller-supplied transaction reference. Must be persisted before the call and
    /// reused verbatim on every retry — Providus deduplicates on this value, so a fresh
    /// reference on retry defeats the guard and risks disbursing twice.
    /// </summary>
    public string? TransactionReference { get; set; }
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

    /// <summary>
    /// The beneficiary name as resolved by NIP name enquiry, where one was performed.
    /// This is the name the funds were actually sent to.
    /// </summary>
    public string? ResolvedBeneficiaryName { get; set; }

    /// <summary>
    /// The NIBSS session ID returned for a NIP transfer, used when tracing with the bank.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// True when the outcome could not be established — the transfer may or may not have
    /// completed. Callers must requery rather than assuming failure, and must never retry
    /// with a fresh reference.
    /// </summary>
    public bool IsIndeterminate { get; set; }
}

#endregion
