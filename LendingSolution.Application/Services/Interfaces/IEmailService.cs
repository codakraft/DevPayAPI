using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Interface for email service operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send OTP via email
    /// </summary>
    /// <param name="emailAddress">Recipient email address</param>
    /// <param name="otp">One-time password</param>
    /// <param name="purpose">Purpose of the OTP (e.g., "Email Verification", "Account Recovery")</param>
    /// <param name="senderName">Optional sender name (defaults to configured value)</param>
    /// <param name="senderEmail">Optional sender email (defaults to configured value)</param>
    Task<bool> SendOtpEmailAsync(string emailAddress, string otp, string purpose = "Email Verification", string? senderName = null, string? senderEmail = null);

    /// <summary>
    /// Send general email
    /// </summary>
    /// <param name="emailAddress">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body (HTML or plain text)</param>
    /// <param name="senderName">Optional sender name (defaults to configured value)</param>
    /// <param name="senderEmail">Optional sender email (defaults to configured value)</param>
    Task<bool> SendEmailAsync(string emailAddress, string subject, string body, string? senderName = null, string? senderEmail = null);
    
    /// <summary>
    /// Send loan offer letter via email with PDF attachment link
    /// </summary>
    /// <param name="offerLetter">Offer letter details</param>
    Task<bool> SendOfferLetterEmailAsync(OfferLetterDto offerLetter);
    
    /// <summary>
    /// Send loan disbursement notification email
    /// </summary>
    /// <param name="emailAddress">Recipient email address</param>
    /// <param name="borrowerName">Borrower's full name</param>
    /// <param name="amount">Disbursed amount</param>
    /// <param name="disbursementReference">Disbursement reference number</param>
    Task<bool> SendDisbursementNotificationAsync(string emailAddress, string borrowerName, decimal amount, string disbursementReference);
}
