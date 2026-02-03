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
    /// <param name="attachmentBytes">Optional PDF attachment as byte array</param>
    /// <param name="attachmentFileName">Optional attachment file name</param>
    Task<bool> SendEmailAsync(string emailAddress, string subject, string body, string? senderName = null, string? senderEmail = null, byte[]? attachmentBytes = null, string? attachmentFileName = null);
    
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
    
    /// <summary>
    /// Send image re-upload request email
    /// </summary>
    /// <param name="emailAddress">Recipient email address</param>
    /// <param name="borrowerName">Borrower's full name</param>
    /// <param name="reason">Reason for requesting new images</param>
    /// <param name="reuploadUrl">URL where borrower can re-upload images</param>
    Task<bool> SendImageReuploadRequestAsync(string emailAddress, string borrowerName, string reason, string reuploadUrl);
    
    /// <summary>
    /// Send loan application summary email after submission
    /// </summary>
    /// <param name="emailAddress">Recipient email address</param>
    /// <param name="borrowerName">Borrower's full name</param>
    /// <param name="loanAmount">Requested loan amount</param>
    /// <param name="tenor">Loan tenor in months</param>
    /// <param name="monthlyRepayment">Monthly repayment amount</param>
    /// <param name="totalRepayment">Total repayment amount</param>
    /// <param name="productName">Loan product name</param>
    /// <param name="companyName">Company name</param>
    /// <param name="offerLetter">Optional offer letter to attach as PDF</param>
    Task<bool> SendLoanApplicationSummaryEmailAsync(string emailAddress, string borrowerName, decimal loanAmount, int tenor, decimal monthlyRepayment, decimal totalRepayment, string productName, string companyName, OfferLetterDto? offerLetter = null);
}
