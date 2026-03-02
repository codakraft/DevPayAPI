using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

// ─────────────────────────────────────────────────────────────────────────────
// 1. Generate Mandate (Setup)
// POST /echannelsvc/echannel/mandate/setup
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request to create a new Direct Debit mandate via Remita's echannel API.
/// </summary>
public class DirectDebitGenerateMandateRequestDto
{
    /// <summary>Your system's unique reference for this mandate.</summary>
    [Required]
    public string MandateRef { get; set; } = string.Empty;

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

    /// <summary>Optional description / narration.</summary>
    public string? Description { get; set; }
}

/// <summary>
/// Top-level response from Remita's mandate setup endpoint.
/// </summary>
public class DirectDebitGenerateMandateResponseDto
{
    [JsonPropertyName("statuscode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

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
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
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
}

// ─────────────────────────────────────────────────────────────────────────────
// 3. Complete Mandate Activation – Validate OTP
// POST /echannelsvc/echannel/mandate/validateAuthorization
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Request to validate the OTP entered by the mandate holder, activating the mandate.
/// </summary>
public class DirectDebitValidateAuthorizationDto
{
    /// <summary>Mandate ID returned from the setup call.</summary>
    [Required]
    public string MandateId { get; set; } = string.Empty;

    /// <summary>OTP received by the payer.</summary>
    [Required]
    public string Otp { get; set; } = string.Empty;

    /// <summary>Payer's registered phone number.</summary>
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;
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
