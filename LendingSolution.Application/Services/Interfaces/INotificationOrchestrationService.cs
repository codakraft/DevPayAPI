using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Orchestrates notification delivery across multiple channels with wallet management
/// </summary>
public interface INotificationOrchestrationService
{
    /// <summary>
    /// Sends a notification through specified channels with automatic wallet validation and fee deduction
    /// </summary>
    /// <param name="request">Notification request with channel, content, and recipients</param>
    /// <returns>Result indicating which channels succeeded and fees deducted</returns>
    Task<SendNotificationResult> SendNotificationAsync(SendNotificationRequest request);
    
    /// <summary>
    /// Convenience method for sending OTP notifications
    /// </summary>
    /// <param name="companyId">Company ID for wallet operations</param>
    /// <param name="email">Recipient email address (can be null if SMS-only)</param>
    /// <param name="phoneNumber">Recipient phone number (can be null if email-only)</param>
    /// <param name="otp">The OTP code to send</param>
    /// <param name="type">Type of OTP (Email verification, BVN verification, etc.)</param>
    /// <param name="preferredChannel">Preferred delivery channel</param>
    /// <param name="senderName">Sender name (company or app name)</param>
    /// <returns>Result indicating success and fees deducted</returns>
    Task<SendNotificationResult> SendOtpAsync(
        Guid companyId,
        string? email,
        string? phoneNumber,
        string otp,
        NotificationType type,
        NotificationChannel preferredChannel,
        string? senderName = null);
}
