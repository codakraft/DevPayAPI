using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Dtos;

// ========== Request DTOs ==========

/// <summary>
/// Request to generate and send an OTP
/// </summary>
public class GenerateOtpRequest
{
    /// <summary>
    /// Type of OTP to generate
    /// </summary>
    public OtpType Type { get; set; }
    
    /// <summary>
    /// Recipient identifier (email, phone, user ID)
    /// </summary>
    public required string RecipientIdentifier { get; set; }
    
    /// <summary>
    /// Company ID (for billing and tracking)
    /// </summary>
    public Guid? CompanyId { get; set; }
    
    /// <summary>
    /// Related entity ID (e.g., ApplicationId, LoanId)
    /// </summary>
    public Guid? RelatedEntityId { get; set; }
    
    /// <summary>
    /// Related entity type (e.g., "BorrowerApplication", "Loan")
    /// </summary>
    public string? RelatedEntityType { get; set; }
    
    /// <summary>
    /// Preferred delivery channel
    /// </summary>
    public NotificationChannel DeliveryChannel { get; set; }
    
    /// <summary>
    /// Sender name for notifications (company name)
    /// </summary>
    public string? SenderName { get; set; }
    
    /// <summary>
    /// Custom purpose description
    /// </summary>
    public string? CustomPurpose { get; set; }
    
    /// <summary>
    /// Override default code length
    /// </summary>
    public int? CodeLengthOverride { get; set; }
    
    /// <summary>
    /// Override default expiry minutes
    /// </summary>
    public int? ExpiryMinutesOverride { get; set; }
    
    /// <summary>
    /// Override default max attempts
    /// </summary>
    public int? MaxAttemptsOverride { get; set; }
    
    /// <summary>
    /// IP address of the requester (for security tracking)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent of the requester
    /// </summary>
    public string? UserAgent { get; set; }
    
    /// <summary>
    /// Who initiated the request ("System", UserId)
    /// </summary>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Request to validate an OTP
/// </summary>
public class ValidateOtpRequest
{
    /// <summary>
    /// Type of OTP being validated
    /// </summary>
    public OtpType Type { get; set; }
    
    /// <summary>
    /// Recipient identifier (email, phone, user ID)
    /// </summary>
    public required string RecipientIdentifier { get; set; }
    
    /// <summary>
    /// OTP code entered by user
    /// </summary>
    public required string Code { get; set; }
    
    /// <summary>
    /// IP address of the validator (for security tracking)
    /// </summary>
    public string? IpAddress { get; set; }
    
    /// <summary>
    /// User agent of the validator
    /// </summary>
    public string? UserAgent { get; set; }
}

/// <summary>
/// Request to resend an OTP
/// </summary>
public class ResendOtpRequest
{
    /// <summary>
    /// Type of OTP to resend
    /// </summary>
    public OtpType Type { get; set; }
    
    /// <summary>
    /// Recipient identifier (email, phone, user ID)
    /// </summary>
    public required string RecipientIdentifier { get; set; }
    
    /// <summary>
    /// Company ID (for billing)
    /// </summary>
    public Guid? CompanyId { get; set; }
    
    /// <summary>
    /// Preferred delivery channel
    /// </summary>
    public NotificationChannel? DeliveryChannel { get; set; }
    
    /// <summary>
    /// Sender name for notifications
    /// </summary>
    public string? SenderName { get; set; }
}

// ========== Response DTOs ==========

/// <summary>
/// Result of OTP generation
/// </summary>
public class GenerateOtpResult
{
    /// <summary>
    /// Whether generation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// OTP ID for tracking
    /// </summary>
    public Guid OtpId { get; set; }
    
    /// <summary>
    /// When the OTP expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Notification delivery result
    /// </summary>
    public SendNotificationResult? NotificationResult { get; set; }
    
    /// <summary>
    /// Error message if generation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Warning messages (e.g., rate limit approaching)
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    public static GenerateOtpResult Succeed(Guid otpId, DateTime expiresAt, SendNotificationResult notificationResult)
        => new() { Success = true, OtpId = otpId, ExpiresAt = expiresAt, NotificationResult = notificationResult };
    
    public static GenerateOtpResult Fail(string errorMessage)
        => new() { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Result of OTP validation
/// </summary>
public class ValidateOtpResult
{
    /// <summary>
    /// Whether validation was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// OTP ID that was validated
    /// </summary>
    public Guid? OtpId { get; set; }
    
    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Remaining attempts before lockout
    /// </summary>
    public int? RemainingAttempts { get; set; }

    /// <summary>
    /// Why validation failed (an <c>ErrorCodes.Otp*</c> value); null on success or an internal error
    /// </summary>
    public string? ErrorCode { get; set; }
    
    public static ValidateOtpResult Succeed(Guid otpId)
        => new() { Success = true, OtpId = otpId };
    
    public static ValidateOtpResult Fail(string errorMessage, int? remainingAttempts = null, string? errorCode = null)
        => new() { Success = false, ErrorMessage = errorMessage, RemainingAttempts = remainingAttempts, ErrorCode = errorCode };
}

/// <summary>
/// Result of OTP resend
/// </summary>
public class ResendOtpResult
{
    /// <summary>
    /// Whether resend was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// OTP ID (new if regenerated, existing if resent)
    /// </summary>
    public Guid OtpId { get; set; }
    
    /// <summary>
    /// Whether a new OTP was generated (vs reusing existing)
    /// </summary>
    public bool WasRegenerated { get; set; }
    
    /// <summary>
    /// When the OTP expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Notification delivery result
    /// </summary>
    public SendNotificationResult? NotificationResult { get; set; }
    
    /// <summary>
    /// Error message if resend failed
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    public static ResendOtpResult Succeed(Guid otpId, bool wasRegenerated, DateTime expiresAt, SendNotificationResult notificationResult)
        => new() { Success = true, OtpId = otpId, WasRegenerated = wasRegenerated, ExpiresAt = expiresAt, NotificationResult = notificationResult };
    
    public static ResendOtpResult Fail(string errorMessage)
        => new() { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Current OTP status for a recipient
/// </summary>
public class OtpStatusResult
{
    /// <summary>
    /// Whether an active OTP exists
    /// </summary>
    public bool HasActiveOtp { get; set; }
    
    /// <summary>
    /// Active OTP ID
    /// </summary>
    public Guid? OtpId { get; set; }
    
    /// <summary>
    /// When the OTP expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Remaining validation attempts
    /// </summary>
    public int? RemainingAttempts { get; set; }
    
    /// <summary>
    /// Whether the OTP is locked
    /// </summary>
    public bool IsLocked { get; set; }
    
    /// <summary>
    /// Whether rate limit has been reached
    /// </summary>
    public bool RateLimitReached { get; set; }
    
    /// <summary>
    /// When rate limit resets
    /// </summary>
    public DateTime? RateLimitResetsAt { get; set; }
}

/// <summary>
/// OTP analytics data
/// </summary>
public class OtpAnalytics
{
    public int TotalGenerated { get; set; }
    public int TotalValidated { get; set; }
    public int TotalFailed { get; set; }
    public int TotalExpired { get; set; }
    public int TotalLocked { get; set; }
    public decimal SuccessRate { get; set; }
    public decimal AverageValidationTimeSeconds { get; set; }
    public Dictionary<OtpType, int> GenerationsByType { get; set; } = new();
    public Dictionary<NotificationChannel, int> DeliveriesByChannel { get; set; } = new();
}

/// <summary>
/// Filter for OTP analytics queries
/// </summary>
public class OtpAnalyticsFilter
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? CompanyId { get; set; }
    public OtpType? Type { get; set; }
}
