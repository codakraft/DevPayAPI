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
    /// Module name (e.g., PAYDAYLOAN)
    /// </summary>
    [MaxLength(100)]
    public string? ModuleName { get; set; }

    /// <summary>
    /// Whether notification was sent
    /// </summary>
    public bool NotificationSent { get; set; }

    /// <summary>
    /// Net salary of customer
    /// </summary>
    public decimal? NetSalary { get; set; }

    /// <summary>
    /// Total credit amount
    /// </summary>
    public decimal? TotalCredit { get; set; }

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
    /// Payment date from Remita
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Payment status (e.g., NEW, SUCCESS, FAILED)
    /// </summary>
    [MaxLength(50)]
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Raw JSON payload from webhook for reference
    /// </summary>
    public string? RawPayload { get; set; }

    /// <summary>
    /// Original JSON payload from webhook request body
    /// </summary>
    public string? Payload { get; set; }
}
