using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// Request to send a notification through the orchestration service
/// </summary>
public class SendNotificationRequest
{
    /// <summary>
    /// Company ID for wallet balance checks and fee deductions
    /// </summary>
    public Guid CompanyId { get; set; }
    
    /// <summary>
    /// Type of notification being sent
    /// </summary>
    public NotificationType Type { get; set; }
    
    /// <summary>
    /// Delivery channel(s) and priority
    /// </summary>
    public NotificationChannel Channel { get; set; }
    
    /// <summary>
    /// Recipient email address (required for email channels)
    /// </summary>
    public string? EmailAddress { get; set; }
    
    /// <summary>
    /// Recipient phone number (required for SMS channels)
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Main content: full message body (used for SMS and as email fallback).
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Raw OTP code only (e.g. "220627"), used to render the email OTP boxes.
    /// Separate from Content, which is the full human-readable sentence.
    /// </summary>
    public string? OtpCode { get; set; }
    
    /// <summary>
    /// Email subject line (optional, auto-generated if not provided)
    /// </summary>
    public string? Subject { get; set; }
    
    /// <summary>
    /// Sender name (company name or app name)
    /// </summary>
    public string? SenderName { get; set; }
    
    /// <summary>
    /// Attachment bytes for emails (e.g., offer letter PDF)
    /// </summary>
    public byte[]? AttachmentBytes { get; set; }
    
    /// <summary>
    /// Attachment file name
    /// </summary>
    public string? AttachmentFileName { get; set; }
    
    /// <summary>
    /// Whether to throw exception if primary channel fails
    /// </summary>
    public bool ThrowOnPrimaryFailure { get; set; } = true;
    
    /// <summary>
    /// Whether to throw exception if secondary channel fails
    /// </summary>
    public bool ThrowOnSecondaryFailure { get; set; } = false;
}

/// <summary>
/// Result of notification sending operation
/// </summary>
public class SendNotificationResult
{
    /// <summary>
    /// Whether email was sent successfully
    /// </summary>
    public bool EmailSent { get; set; }
    
    /// <summary>
    /// Whether SMS was sent successfully
    /// </summary>
    public bool SmsSent { get; set; }
    
    /// <summary>
    /// Fee deducted for email (0 if not sent or failed)
    /// </summary>
    public decimal EmailFeeDeducted { get; set; }
    
    /// <summary>
    /// Fee deducted for SMS (0 if not sent or failed)
    /// </summary>
    public decimal SmsFeeDeducted { get; set; }
    
    /// <summary>
    /// Total fees deducted across all channels
    /// </summary>
    public decimal TotalFeeDeducted { get; set; }
    
    /// <summary>
    /// Warning messages (e.g., secondary channel skipped due to insufficient balance)
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// Whether at least one channel was successful
    /// </summary>
    public bool Success => EmailSent || SmsSent;
}
