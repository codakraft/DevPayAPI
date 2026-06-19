using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

// ─────────────────────────────────────────────────────────────────────────────
// 1. Generate Mandate (Setup)
// POST /echannelsvc/echannel/mandate/setup
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request to create a new Direct Debit mandate via Remita's echannel API.
/// merchantId, serviceTypeId, requestId, hash are injected server-side from settings.
/// </summary>
public class DirectDebitGenerateMandateRequestDto
{
    /// <summary>Full name of the account holder.</summary>
    [Required]
    public string PayerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PayerEmail { get; set; } = string.Empty;

    [Required]
    public string PayerPhone { get; set; } = string.Empty;

    /// <summary>Bank code of the payer's bank (e.g. "044" for Access Bank).</summary>
    [Required]
    public string PayerBankCode { get; set; } = string.Empty;

    /// <summary>10-digit NUBAN account number.</summary>
    [Required]
    [StringLength(10, MinimumLength = 10)]
    public string PayerAccountNumber { get; set; } = string.Empty;

    /// <summary>Amount in Naira to be debited on each collection date.</summary>
    [Required]
    [Range(1, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>First scheduled debit date (dd/MM/yyyy).</summary>
    [Required]
    public string StartDate { get; set; } = string.Empty;

    /// <summary>Last scheduled debit date (dd/MM/yyyy).</summary>
    [Required]
    public string EndDate { get; set; } = string.Empty;

    /// <summary>
    /// Mandate type: "DD" (Direct Debit) or "SO" (Standing Order).
    /// Defaults to "DD".
    /// </summary>
    public string MandateType { get; set; } = "DD";

    /// <summary>
    /// Debit frequency: DAILY, WEEKLY, MONTHLY, QUARTERLY, ANNUALLY, etc.
    /// Defaults to "MONTHLY".
    /// </summary>
    public string Frequency { get; set; } = "MONTHLY";

    /// <summary>Optional description / narration.</summary>
    public string? Description { get; set; }
}

/// <summary>
/// Top-level response from Remita's mandate setup endpoint.
/// Remita returns fields both at root level and nested in 'data'.
/// </summary>
public class DirectDebitGenerateMandateResponseDto
{
    [JsonPropertyName("statuscode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>Root-level mandateId (actual Remita response format)</summary>
    [JsonPropertyName("mandateId")]
    public string? MandateId { get; set; }

    /// <summary>Root-level requestId (actual Remita response format)</summary>
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    /// <summary>Nested data object (for compatibility)</summary>
    [JsonPropertyName("data")]
    public DirectDebitGenerateMandateDataDto? Data { get; set; }
}

public class DirectDebitGenerateMandateDataDto
{
    [JsonPropertyName("mandateId")]
    public string? MandateId { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// 2. Initiate Mandate Activation – Request OTP
// POST /echannelsvc/echannel/mandate/requestAuthorization
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request to trigger OTP delivery to the mandate holder's phone.
/// </summary>
public class DirectDebitRequestAuthorizationDto
{
    /// <summary>Mandate ID returned from the setup call.</summary>
    [Required]
    public string MandateId { get; set; } = string.Empty;

    /// <summary>Payer's registered phone number.</summary>
    public string PhoneNumber { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
}

/// <summary>
/// A single auth parameter descriptor returned by Remita for requestAuthorization.
/// Tells the frontend which inputs are required (e.g. OTP, card digits).
/// </summary>
public class RemitaAuthParamDescriptorDto
{
    [JsonPropertyName("param1")]
    public string? Param1 { get; set; }

    [JsonPropertyName("label1")]
    public string? Label1 { get; set; }

    [JsonPropertyName("description1")]
    public string? Description1 { get; set; }

    [JsonPropertyName("param2")]
    public string? Param2 { get; set; }

    [JsonPropertyName("label2")]
    public string? Label2 { get; set; }

    [JsonPropertyName("description2")]
    public string? Description2 { get; set; }
}

/// <summary>
/// Response after requesting OTP from Remita.
/// </summary>
public class DirectDebitRequestAuthorizationResponseDto
{
    [JsonPropertyName("statuscode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    [JsonPropertyName("mandateId")]
    public string? MandateId { get; set; }

    /// <summary>
    /// Remita transaction reference returned alongside the OTP dispatch.
    /// This is the value that must be submitted as remitaTransRef in validateAuthorization.
    /// </summary>
    [JsonPropertyName("remitaTransRef")]
    public string? RemitaTransRef { get; set; }

    /// <summary>
    /// Describes the auth inputs the borrower must supply (e.g. OTP, last 4 card digits).
    /// </summary>
    [JsonPropertyName("authParams")]
    public List<RemitaAuthParamDescriptorDto>? AuthParams { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// 3. Complete Mandate Activation – Validate OTP
// POST /echannelsvc/echannel/mandate/validateAuthorization
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// A single auth param value submitted to Remita for validateAuthorization.
/// </summary>
public class RemitaAuthParamValueDto
{
    [JsonPropertyName("param1")]
    public string? Param1 { get; set; }

    [JsonPropertyName("param2")]
    public string? Param2 { get; set; }

    [JsonPropertyName("value")]
    [Required]
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Request to validate the OTP entered by the mandate holder, activating the mandate.
/// Matches Remita's validateAuthorization payload format.
/// </summary>
public class DirectDebitValidateAuthorizationDto
{
    /// <summary>
    /// Transaction reference returned from the requestAuthorization step
    /// (the requestId sent during the OTP request).
    /// </summary>
    [Required]
    public string RemitaTransRef { get; set; } = string.Empty;

    /// <summary>Auth param values provided by the mandate holder (OTP, card digits, etc.).</summary>
    [Required]
    public List<RemitaAuthParamValueDto> AuthParams { get; set; } = new();
}

/// <summary>
/// Response after validating OTP. A success here means the mandate is active.
/// </summary>
public class DirectDebitValidateAuthorizationResponseDto
{
    [JsonPropertyName("statuscode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("mandateRef")]
    public string? MandateRef { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// 4. Stop / Cancel a Direct Debit Mandate
// POST /echannelsvc/echannel/mandate/stop
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request to cancel an active Direct Debit mandate.
/// </summary>
public class DirectDebitStopMandateRequestDto
{
    /// <summary>Mandate ID or reference to cancel.</summary>
    [Required]
    public string MandateId { get; set; } = string.Empty;
}

/// <summary>
/// Response after stopping a Direct Debit mandate.
/// </summary>
public class DirectDebitStopMandateResponseDto
{
    [JsonPropertyName("statuscode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
