using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class RemitaWebhookNotificationDto
{
    [Required]
    [StringLength(50)]
    public string NotificationType { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string TransactionRef { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RemitaTransRef { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string MerchantId { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ServiceTypeId { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? MandateId { get; set; }

    [StringLength(100)]
    public string? Hash { get; set; }

    [StringLength(500)]
    public string? ResponseMessage { get; set; }

    [StringLength(10)]
    public string? ResponseCode { get; set; }

    public Dictionary<string, object>? AdditionalData { get; set; }
}
