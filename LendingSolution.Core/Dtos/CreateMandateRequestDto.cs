using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

public class CreateMandateRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [Range(1, 60)]
    public int TenorInMonths { get; set; }

    [Required]
    [StringLength(10)]
    public string Frequency { get; set; } = "Monthly";

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(50)]
    public string PayerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PayerEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PayerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string PayerBankCode { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string PayerAccount { get; set; } = string.Empty;

    [Required]
    [StringLength(2)]
    public string MandateType { get; set; } = "SO"; // Standing Order

    [StringLength(200)]
    public string? Description { get; set; }
}

// Connect Gateway Mandate Activation DTOs
public class ActivateMandateViaPaymentRequestDto
{
    [JsonPropertyName("rrr")]
    [Required]
    [StringLength(100)]
    public string Rrr { get; set; } = string.Empty;
    
    [JsonPropertyName("transactionRef")]
    [Required]
    [StringLength(100)]
    public string TransactionRef { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("metadata")]
    public ActivateMandateMetadataDto? Metadata { get; set; }
}

public class ActivateMandateMetadataDto
{
    [JsonPropertyName("payerAccountNumber")]
    public string PayerAccountNumber { get; set; } = string.Empty;
}

public class ActivateMandateViaPaymentResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
