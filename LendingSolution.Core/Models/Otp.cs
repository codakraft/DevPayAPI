using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Models;

/// <summary>
/// OTP (One-Time Password) entity for secure verification
/// </summary>
public class Otp
{
    /// <summary>
    /// Unique identifier for the OTP
    /// </summary>
    public Guid Id { get; set; }
    
    // ========== Classification ==========
    
    /// <summary>
    /// Type of OTP (Email, BVN, Phone, etc.)
    /// </summary>
    public OtpType Type { get; set; }
    
    /// <summary>
    /// Human-readable purpose (e.g., "Email Verification", "BVN Verification")
    /// </summary>
    public string Purpose { get; set; } = string.Empty;
    
    // ========== Recipient ==========
    
    /// <summary>
    /// Identifier for the recipient (email, phone number, user ID)
    /// </summary>
    public string RecipientIdentifier { get; set; } = string.Empty;
    
    // ========== Code Details ==========
    
    /// <summary>
    /// The actual OTP code
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Length of the OTP code (4, 6, 8 digits)
    /// </summary>
    public int CodeLength { get; set; }
    
    // ========== Timing ==========
    
    /// <summary>
    /// When the OTP was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    
    /// <summary>
    /// When the OTP expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Expiry duration in minutes (for audit/reporting)
    /// </summary>
    public int ExpiryMinutes { get; set; }
    
    // ========== Security ==========
    
    /// <summary>
    /// Number of validation attempts made
    /// </summary>
    public int AttemptCount { get; set; }
    
    /// <summary>
    /// Maximum allowed validation attempts
    /// </summary>
    public int MaxAttempts { get; set; }
    
    /// <summary>
    /// Whether the OTP is locked due to too many failed attempts
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// When the OTP was locked
    /// </summary>
    public DateTime? LockedAt { get; set; }
    
    // ========== Status ==========
    
    /// <summary>
    /// Whether the OTP has been successfully used
    /// </summary>
    public bool IsUsed { get; set; }
    
    /// <summary>
    /// When the OTP was successfully used
    /// </summary>
    public DateTime? UsedAt { get; set; }
    
    /// <summary>
    /// Whether the OTP was manually invalidated
    /// </summary>
    public bool IsInvalidated { get; set; }
    
    /// <summary>
    /// When the OTP was invalidated
    /// </summary>
    public DateTime? InvalidatedAt { get; set; }
    
    /// <summary>
    /// Reason for invalidation (e.g., "User cancelled", "Wrong recipient")
    /// </summary>
    public string? InvalidationReason { get; set; }
    
    // ========== Delivery Tracking ==========
    
    /// <summary>
    /// Channel used for delivery (Email, SMS, Both)
    /// </summary>
    public NotificationChannel DeliveryChannel { get; set; }
    
    /// <summary>
    /// Whether the OTP was successfully delivered
    /// </summary>
    public bool WasDelivered { get; set; }
    
    /// <summary>
    /// When the OTP was delivered
    /// </summary>
    public DateTime? DeliveredAt { get; set; }
    
    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? DeliveryError { get; set; }
    
    // ========== Context ==========
    
    /// <summary>
    /// Company ID associated with this OTP (for billing/tracking)
    /// </summary>
    public Guid? CompanyId { get; set; }
    
    /// <summary>
    /// ID of the related entity (e.g., LoanId, ApplicationId)
    /// </summary>
    public Guid? RelatedEntityId { get; set; }
    
    /// <summary>
    /// Type of the related entity (e.g., "BorrowerApplication", "User")
    /// </summary>
    public string? RelatedEntityType { get; set; }
    
    // ========== Audit ==========
    
    /// <summary>
    /// Who created this OTP ("System", UserId)
    /// </summary>
    public string? CreatedBy { get; set; }
    
    /// <summary>
    /// IP address of the requester
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent of the requester
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// When the record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// When the record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
