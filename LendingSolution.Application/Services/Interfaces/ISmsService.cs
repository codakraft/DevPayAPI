namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Interface for SMS service operations
/// </summary>
public interface ISmsService
{
    /// <summary>
    /// Send OTP via SMS
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="otp">One-time password</param>
    /// <param name="purpose">Purpose of the OTP (e.g., "BVN Verification", "Account Recovery")</param>
    Task<bool> SendOtpSmsAsync(string phoneNumber, string otp, string purpose = "BVN Verification");

    /// <summary>
    /// Send general SMS
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="message">SMS message</param>
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
