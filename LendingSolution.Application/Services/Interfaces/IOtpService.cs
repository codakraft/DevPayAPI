using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Service for managing OTP (One-Time Password) generation, validation, and lifecycle
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates an OTP and sends it via the notification orchestrator
    /// </summary>
    /// <param name="request">OTP generation request with recipient and delivery details</param>
    /// <returns>Result containing OTP ID, expiry time, and delivery status</returns>
    Task<GenerateOtpResult> GenerateAndSendOtpAsync(GenerateOtpRequest request);
    
    /// <summary>
    /// Validates an OTP code entered by the user
    /// </summary>
    /// <param name="request">Validation request with recipient, code, and type</param>
    /// <returns>Result indicating success/failure and remaining attempts</returns>
    Task<ValidateOtpResult> ValidateOtpAsync(ValidateOtpRequest request);
    
    /// <summary>
    /// Resends an existing OTP or generates a new one if expired
    /// </summary>
    /// <param name="request">Resend request with recipient and type</param>
    /// <returns>Result indicating whether OTP was resent or regenerated</returns>
    Task<ResendOtpResult> ResendOtpAsync(ResendOtpRequest request);
    
    /// <summary>
    /// Gets the current OTP status for a recipient
    /// </summary>
    /// <param name="recipientIdentifier">Recipient (email, phone, etc.)</param>
    /// <param name="type">OTP type</param>
    /// <returns>Status including expiry, remaining attempts, and rate limit info</returns>
    Task<OtpStatusResult> GetOtpStatusAsync(string recipientIdentifier, OtpType type);
    
    /// <summary>
    /// Manually invalidates an OTP (user cancelled, wrong recipient, etc.)
    /// </summary>
    /// <param name="otpId">OTP ID to invalidate</param>
    /// <param name="reason">Reason for invalidation</param>
    /// <returns>True if invalidated successfully</returns>
    Task<bool> InvalidateOtpAsync(Guid otpId, string reason);
    
    /// <summary>
    /// Cleanup expired OTPs (background job)
    /// </summary>
    /// <param name="daysOld">Delete OTPs older than this many days (default 30)</param>
    /// <returns>Number of OTPs deleted</returns>
    Task<int> CleanupExpiredOtpsAsync(int daysOld = 30);
    
    /// <summary>
    /// Get OTP analytics for reporting
    /// </summary>
    /// <param name="startDate">Start date for analytics</param>
    /// <param name="endDate">End date for analytics</param>
    /// <param name="companyId">Optional company filter</param>
    /// <returns>Analytics data including success rates, delivery stats, etc.</returns>
    Task<OtpAnalytics> GetOtpAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? companyId = null);
}
