namespace LendingSolution.Core.Enum;

/// <summary>
/// Defines the notification delivery channels and their priority
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Send via email only
    /// </summary>
    Email,
    
    /// <summary>
    /// Send via SMS only
    /// </summary>
    SMS,
    
    /// <summary>
    /// Send via email (required), then SMS (optional if balance allows)
    /// </summary>
    EmailPrimary,
    
    /// <summary>
    /// Send via SMS (required), then Email (optional if balance allows)
    /// </summary>
    SMSPrimary,
    
    /// <summary>
    /// Both email and SMS required
    /// </summary>
    Both
}
