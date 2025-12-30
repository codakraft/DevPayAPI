using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Stores Remita loan collection notifications received from webhook
/// </summary>
public class RemitaLoanCollectionNotification : Base
{
    /// <summary>
    /// Unique identifier from Remita
    /// </summary>
    public long RemitaId { get; set; }

    /// <summary>
    /// Amount collected
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Status code from Remita
    /// </summary>
    [MaxLength(50)]
    public string? StatusCode { get; set; }

    /// <summary>
    /// Module name (e.g., PAYDAYLOAN)
    /// </summary>
    [MaxLength(100)]
    public string? ModuleName { get; set; }

    /// <summary>
    /// Whether notification was sent
    /// </summary>
    public bool NotificationSent { get; set; }

    /// <summary>
    /// Date notification was sent
    /// </summary>
    public DateTime? DateNotificationSent { get; set; }

    /// <summary>
    /// Whether first notification was sent
    /// </summary>
    public bool? FirstNotificationSent { get; set; }

    /// <summary>
    /// Date first notification was sent
    /// </summary>
    public DateTime? DateFirstNotificationSent { get; set; }

    /// <summary>
    /// Net salary of customer
    /// </summary>
    public decimal? NetSalary { get; set; }

    /// <summary>
    /// Total credit amount
    /// </summary>
    public decimal? TotalCredit { get; set; }

    /// <summary>
    /// Customer phone number
    /// </summary>
    [MaxLength(20)]
    public string? CustomerPhoneNumber { get; set; }

    /// <summary>
    /// Mandate reference
    /// </summary>
    [MaxLength(100)]
    [Required]
    public string MandateRef { get; set; } = string.Empty;

    /// <summary>
    /// Balance due
    /// </summary>
    public decimal? BalanceDue { get; set; }

    /// <summary>
    /// Customer ID
    /// </summary>
    [MaxLength(100)]
    public string? CustomerId { get; set; }

    /// <summary>
    /// Request ID
    /// </summary>
    [MaxLength(100)]
    public string? RequestId { get; set; }

    /// <summary>
    /// Payment date from Remita
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Payment status (e.g., NEW, SUCCESS, FAILED)
    /// </summary>
    [MaxLength(50)]
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Reason for status
    /// </summary>
    [MaxLength(500)]
    public string? StatusReason { get; set; }

    /// <summary>
    /// Raw JSON payload from webhook for reference
    /// </summary>
    public string? RawPayload { get; set; }
}
