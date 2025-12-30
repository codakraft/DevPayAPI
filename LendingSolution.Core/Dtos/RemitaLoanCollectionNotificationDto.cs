using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO for Remita loan collection notification webhook payload
/// </summary>
public class RemitaLoanCollectionNotificationDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("statusCode")]
    public string? StatusCode { get; set; }

    [JsonPropertyName("modulename")]
    public string? ModuleName { get; set; }

    [JsonPropertyName("notificationSent")]
    public bool NotificationSent { get; set; }

    [JsonPropertyName("dateNotificationSent")]
    public string? DateNotificationSent { get; set; }

    [JsonPropertyName("firstNotificationSent")]
    public bool? FirstNotificationSent { get; set; }

    [JsonPropertyName("dateFirstNotificationSent")]
    public string? DateFirstNotificationSent { get; set; }

    [JsonPropertyName("netSalary")]
    public decimal? NetSalary { get; set; }

    [JsonPropertyName("totalCredit")]
    public decimal? TotalCredit { get; set; }

    [JsonPropertyName("customerPhoneNumber")]
    public string? CustomerPhoneNumber { get; set; }

    [JsonPropertyName("mandateRef")]
    public string MandateRef { get; set; } = string.Empty;

    [JsonPropertyName("balanceDue")]
    public decimal? BalanceDue { get; set; }

    [JsonPropertyName("customer_id")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("request_id")]
    public string? RequestId { get; set; }

    [JsonPropertyName("payment_date")]
    public string? PaymentDate { get; set; }

    [JsonPropertyName("payment_status")]
    public string? PaymentStatus { get; set; }

    [JsonPropertyName("status_reason")]
    public string? StatusReason { get; set; }
}
