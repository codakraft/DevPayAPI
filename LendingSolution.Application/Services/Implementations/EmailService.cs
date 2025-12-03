using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Email service implementation using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendOtpEmailAsync(string emailAddress, string otp, string purpose = "Email Verification")
    {
        var subject = $"Your {purpose} Code";
        var body = GenerateOtpEmailTemplate(otp, purpose);
        
        return await SendEmailAsync(emailAddress, subject, body);
    }

    public async Task<bool> SendEmailAsync(string emailAddress, string subject, string body)
    {
        try
        {
            // Check if email service is properly configured
            if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || 
                _emailSettings.SmtpHost.Contains("dummy") ||
                string.IsNullOrEmpty(_emailSettings.FromEmail))
            {
                _logger.LogWarning("Email service not configured - simulating email send to {EmailAddress}", emailAddress);
                _logger.LogInformation("EMAIL SIMULATION - To: {EmailAddress}, Subject: {Subject}", emailAddress, subject);
                return true; // Simulate successful send
            }

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
            client.EnableSsl = _emailSettings.EnableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(emailAddress);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {EmailAddress}", emailAddress);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress}", emailAddress);
            return false;
        }
    }

    private string GenerateOtpEmailTemplate(string otp, string purpose)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .otp-code {{ 
            font-size: 32px; 
            font-weight: bold; 
            color: #007bff; 
            text-align: center; 
            letter-spacing: 5px; 
            padding: 20px; 
            background-color: white; 
            border: 2px dashed #007bff; 
            margin: 20px 0; 
        }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>LendingSolution</h1>
        </div>
        <div class='content'>
            <h2>{purpose}</h2>
            <p>Your verification code is:</p>
            <div class='otp-code'>{otp}</div>
            <p><strong>Important:</strong></p>
            <ul>
                <li>This code expires in 10 minutes</li>
                <li>Do not share this code with anyone</li>
                <li>If you didn't request this code, please ignore this email</li>
            </ul>
        </div>
        <div class='footer'>
            <p>This is an automated message, please do not reply.</p>
            <p>&copy; 2025 LendingSolution. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
    
    public async Task<bool> SendOfferLetterEmailAsync(OfferLetterDto offerLetter)
    {
        var subject = $"Loan Offer Letter - {offerLetter.CompanyName}";
        var body = GenerateOfferLetterEmailTemplate(offerLetter);
        
        return await SendEmailAsync(offerLetter.BorrowerEmail, subject, body);
    }
    
    public async Task<bool> SendDisbursementNotificationAsync(string emailAddress, string borrowerName, decimal amount, string disbursementReference)
    {
        var subject = "Loan Disbursement Notification";
        var body = GenerateDisbursementEmailTemplate(borrowerName, amount, disbursementReference);
        
        return await SendEmailAsync(emailAddress, subject, body);
    }
    
    private string GenerateOfferLetterEmailTemplate(OfferLetterDto offer)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }}
        .container {{ max-width: 800px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 30px; text-align: center; }}
        .header h1 {{ margin: 0; }}
        .content {{ padding: 30px; background-color: #ffffff; }}
        .offer-title {{ text-align: center; color: #007bff; font-size: 24px; margin-bottom: 30px; border-bottom: 2px solid #007bff; padding-bottom: 15px; }}
        .section {{ margin-bottom: 25px; }}
        .section-title {{ font-weight: bold; color: #007bff; font-size: 16px; margin-bottom: 10px; border-bottom: 1px solid #ddd; padding-bottom: 5px; }}
        .detail-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px dotted #eee; }}
        .detail-label {{ color: #666; }}
        .detail-value {{ font-weight: bold; color: #333; }}
        .highlight-box {{ background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0; }}
        .amount {{ font-size: 28px; color: #28a745; font-weight: bold; text-align: center; padding: 20px; background-color: #f0fff4; border-radius: 8px; margin: 20px 0; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 5px; margin: 20px 0; }}
        .cta-button {{ display: inline-block; background-color: #007bff; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }}
        .signature-section {{ margin-top: 40px; border-top: 2px solid #333; padding-top: 30px; }}
        .signature-box {{ border: 2px dashed #999; padding: 40px; text-align: center; margin: 20px 0; background-color: #fafafa; }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; background-color: #f5f5f5; }}
        table {{ width: 100%; border-collapse: collapse; }}
        td {{ padding: 10px; vertical-align: top; }}
        .terms {{ font-size: 12px; color: #666; margin-top: 30px; }}
        .terms ol {{ padding-left: 20px; }}
        .terms li {{ margin-bottom: 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{offer.CompanyName}</h1>
            <p>Loan Offer Letter</p>
        </div>
        
        <div class='content'>
            <div class='offer-title'>LOAN OFFER LETTER</div>
            
            <p><strong>Date:</strong> {offer.OfferDate:MMMM dd, yyyy}</p>
            <p><strong>Offer Reference:</strong> LOL-{offer.LoanId.ToString()[..8].ToUpper()}</p>
            
            <div class='section'>
                <div class='section-title'>BORROWER DETAILS</div>
                <table>
                    <tr><td class='detail-label'>Name:</td><td class='detail-value'>{offer.BorrowerName}</td></tr>
                    <tr><td class='detail-label'>Email:</td><td class='detail-value'>{offer.BorrowerEmail}</td></tr>
                    <tr><td class='detail-label'>Address:</td><td class='detail-value'>{offer.BorrowerAddress}</td></tr>
                </table>
            </div>
            
            <div class='section'>
                <div class='section-title'>LOAN DETAILS</div>
                <div class='amount'>₦{offer.LoanAmount:N2}</div>
                <table>
                    <tr><td class='detail-label'>Loan Product:</td><td class='detail-value'>{offer.ProductName}</td></tr>
                    <tr><td class='detail-label'>Purpose:</td><td class='detail-value'>{offer.Purpose}</td></tr>
                    <tr><td class='detail-label'>Loan Tenure:</td><td class='detail-value'>{offer.DurationInMonths} Month(s)</td></tr>
                    <tr><td class='detail-label'>Interest Rate:</td><td class='detail-value'>{offer.InterestRate}% ({offer.InterestComputationBasis})</td></tr>
                    <tr><td class='detail-label'>Total Interest:</td><td class='detail-value'>₦{offer.TotalInterest:N2}</td></tr>
                    <tr><td class='detail-label'>Total Repayment:</td><td class='detail-value'>₦{offer.TotalRepayment:N2}</td></tr>
                    <tr><td class='detail-label'>Monthly Repayment:</td><td class='detail-value'>₦{offer.MonthlyRepayment:N2}</td></tr>
                </table>
            </div>
            
            <div class='section'>
                <div class='section-title'>IMPORTANT DATES</div>
                <table>
                    <tr><td class='detail-label'>Offer Valid Until:</td><td class='detail-value'>{offer.ExpiryDate:MMMM dd, yyyy}</td></tr>
                    <tr><td class='detail-label'>Expected Disbursement:</td><td class='detail-value'>{offer.ExpectedDisbursementDate:MMMM dd, yyyy}</td></tr>
                    <tr><td class='detail-label'>Maturity Date:</td><td class='detail-value'>{offer.ExpectedMaturityDate:MMMM dd, yyyy}</td></tr>
                    <tr><td class='detail-label'>Grace Period:</td><td class='detail-value'>{offer.MoratoriumDays} days</td></tr>
                </table>
            </div>
            
            <div class='highlight-box'>
                <strong>Penalty for Default:</strong> A penalty rate of {offer.PenaltyRate}% will be applied on outstanding principal for late payments.
            </div>
            
            <div class='terms'>
                <div class='section-title'>TERMS AND CONDITIONS</div>
                <ol>
                    <li>This offer is valid for 7 days from the date of issue.</li>
                    <li>The borrower agrees to repay the loan amount plus interest according to the repayment schedule.</li>
                    <li>Late payments will attract a penalty as specified above.</li>
                    <li>The borrower authorizes deduction of loan repayments from their salary account.</li>
                    <li>The borrower confirms that all information provided is accurate and complete.</li>
                    <li>This loan is subject to the successful setup of a direct debit mandate.</li>
                    <li>The lender reserves the right to modify the terms with prior notice.</li>
                    <li>This agreement is governed by the laws of the Federal Republic of Nigeria.</li>
                </ol>
            </div>
            
            <div class='signature-section'>
                <p><strong>ACCEPTANCE OF OFFER</strong></p>
                <p>I, {offer.BorrowerName}, hereby accept the loan offer as detailed above and agree to abide by all terms and conditions.</p>
                
                <div class='signature-box'>
                    <p><strong>BORROWER'S SIGNATURE</strong></p>
                    <br/><br/>
                    <p>_________________________________</p>
                    <p>Signature & Date</p>
                </div>
                
                <p style='text-align: center; margin-top: 20px;'>
                    <strong>Please print this document, sign it, and upload the signed copy to complete your loan application.</strong>
                </p>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Important:</strong> Please review all terms carefully before signing. By signing this document, you are entering into a legally binding agreement.
            </div>
        </div>
        
        <div class='footer'>
            <p>This is an official loan offer from {offer.CompanyName}.</p>
            <p>{offer.CompanyAddress}</p>
            <p>&copy; {DateTime.UtcNow.Year} {offer.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
    
    private string GenerateDisbursementEmailTemplate(string borrowerName, decimal amount, string reference)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 30px; background-color: #f9f9f9; }}
        .amount {{ font-size: 32px; font-weight: bold; color: #28a745; text-align: center; padding: 20px; background-color: white; border: 2px solid #28a745; margin: 20px 0; border-radius: 8px; }}
        .detail-box {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ padding: 20px; text-align: center; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Congratulations!</h1>
            <p>Your Loan Has Been Disbursed</p>
        </div>
        <div class='content'>
            <p>Dear {borrowerName},</p>
            <p>We are pleased to inform you that your loan has been successfully disbursed.</p>
            
            <div class='amount'>₦{amount:N2}</div>
            
            <div class='detail-box'>
                <p><strong>Disbursement Reference:</strong> {reference}</p>
                <p><strong>Date:</strong> {DateTime.UtcNow:MMMM dd, yyyy}</p>
            </div>
            
            <p>The funds have been credited to your designated bank account. Please allow 1-2 business days for the transfer to reflect.</p>
            
            <p><strong>Important Reminders:</strong></p>
            <ul>
                <li>Keep track of your repayment schedule</li>
                <li>Ensure sufficient funds in your salary account for repayments</li>
                <li>Contact us if you have any questions or concerns</li>
            </ul>
            
            <p>Thank you for choosing our lending services.</p>
        </div>
        <div class='footer'>
            <p>This is an automated message, please do not reply.</p>
            <p>&copy; {DateTime.UtcNow.Year} LendingSolution. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
