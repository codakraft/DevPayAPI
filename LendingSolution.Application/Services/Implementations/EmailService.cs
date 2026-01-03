using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Email service implementation using SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IConfiguration configuration)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _configuration = configuration;
        
        // Configure QuestPDF license (Community license for free usage)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<bool> SendOtpEmailAsync(string emailAddress, string otp, string purpose = "Email Verification", string? senderName = null, string? senderEmail = null)
    {
        var subject = $"Your {purpose} Code";
        var body = GenerateOtpEmailTemplate(otp, purpose, senderName ?? _emailSettings.FromName);
        
        return await SendEmailAsync(emailAddress, subject, body, senderName, senderEmail);
    }

    public async Task<bool> SendEmailAsync(string emailAddress, string subject, string body, string? senderName = null, string? senderEmail = null, byte[]? attachmentBytes = null, string? attachmentFileName = null)
    {
        // Use provided sender info or fall back to configured defaults (declare outside try for scope)
        var fromEmail = senderEmail ?? _emailSettings.FromEmail;
        var fromName = senderName ?? _emailSettings.FromName;

        try
        {
            // Check if email service is properly configured
            if (string.IsNullOrEmpty(_emailSettings.SmtpHost) || 
                _emailSettings.SmtpHost.Contains("dummy") ||
                string.IsNullOrEmpty(fromEmail))
            {
                _logger.LogWarning("Email service not configured - simulating email send to {EmailAddress}", emailAddress);
                _logger.LogInformation("EMAIL SIMULATION - To: {EmailAddress}, Subject: {Subject}, From: {FromName} <{FromEmail}>", emailAddress, subject, fromName, fromEmail);
                return true; // Simulate successful send
            }

            using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort);
            client.EnableSsl = _emailSettings.EnableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
            client.Timeout = 10000; // 10 seconds timeout to prevent hanging

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(emailAddress);

            // Add attachment if provided
            if (attachmentBytes != null && !string.IsNullOrEmpty(attachmentFileName))
            {
                var stream = new MemoryStream(attachmentBytes);
                var attachment = new Attachment(stream, attachmentFileName, "application/pdf");
                mailMessage.Attachments.Add(attachment);
            }

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {EmailAddress} from {FromName} <{FromEmail}>", emailAddress, fromName, fromEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {EmailAddress}", emailAddress);
            
            // If email simulation on failure is enabled, simulate the send instead of failing
            if (_emailSettings.AllowEmailSimulationOnFailure)
            {
                _logger.LogWarning("Email sending failed but AllowEmailSimulationOnFailure is enabled - simulating email send to {EmailAddress}", emailAddress);
                _logger.LogInformation("EMAIL SIMULATION (FALLBACK) - To: {EmailAddress}, Subject: {Subject}, From: {FromName} <{FromEmail}>", emailAddress, subject, fromName, fromEmail);
                return true; // Simulate successful send
            }
            
            return false;
        }
    }

    private string GenerateOtpEmailTemplate(string otp, string purpose, string companyName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6; 
            color: #1a1a1a; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 20px;
        }}
        .email-wrapper {{ 
            max-width: 600px; 
            margin: 0 auto; 
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 30px;
            text-align: center;
            position: relative;
        }}
        .header::after {{
            content: '';
            position: absolute;
            bottom: -2px;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, #f093fb 0%, #f5576c 100%);
        }}
        .logo {{ 
            color: white; 
            font-size: 32px; 
            font-weight: 700;
            letter-spacing: 1px;
            margin: 0;
            text-shadow: 0 2px 10px rgba(0,0,0,0.2);
        }}
        .content {{ 
            padding: 50px 40px;
            background: white;
        }}
        .greeting {{ 
            font-size: 24px; 
            font-weight: 600; 
            color: #2d3748;
            margin-bottom: 20px;
        }}
        .message {{ 
            font-size: 16px; 
            color: #4a5568;
            margin-bottom: 30px;
        }}
        .otp-container {{
            background: linear-gradient(135deg, #f6f8fb 0%, #e9ecef 100%);
            border-radius: 12px;
            padding: 30px;
            margin: 30px 0;
            text-align: center;
            border: 2px solid #e2e8f0;
        }}
        .otp-label {{
            font-size: 14px;
            color: #718096;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            margin-bottom: 15px;
        }}
        .otp-code {{ 
            font-size: 80px; 
            font-weight: 900; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
            letter-spacing: 12px; 
            padding: 15px 0;
            font-family: 'Courier New', monospace;
            text-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .timer-badge {{
            display: inline-block;
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            color: white;
            padding: 8px 20px;
            border-radius: 20px;
            font-size: 13px;
            font-weight: 600;
            margin-top: 15px;
        }}
        .security-notice {{
            background: #fff5f5;
            border-left: 4px solid #fc8181;
            padding: 20px;
            border-radius: 8px;
            margin: 30px 0;
        }}
        .security-notice h3 {{
            color: #c53030;
            font-size: 16px;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
        }}
        .security-notice h3::before {{
            content: '🔒';
            margin-right: 8px;
            font-size: 20px;
        }}
        .security-notice ul {{
            margin: 0;
            padding-left: 20px;
            color: #742a2a;
        }}
        .security-notice li {{
            margin: 8px 0;
            font-size: 14px;
        }}
        .help-section {{
            background: #f7fafc;
            padding: 25px;
            border-radius: 8px;
            margin-top: 30px;
            text-align: center;
        }}
        .help-section p {{
            color: #4a5568;
            font-size: 14px;
            margin: 5px 0;
        }}
        .help-section strong {{
            color: #2d3748;
        }}
        .footer {{ 
            background: #2d3748;
            padding: 30px;
            text-align: center;
            color: #a0aec0;
        }}
        .footer p {{
            margin: 8px 0;
            font-size: 13px;
        }}
        .footer-links {{
            margin: 15px 0;
        }}
        .footer-links a {{
            color: #a0aec0;
            text-decoration: none;
            margin: 0 10px;
            font-size: 12px;
        }}
        .footer-links a:hover {{
            color: #cbd5e0;
        }}
        @media only screen and (max-width: 600px) {{
            .content {{ padding: 30px 20px; }}
            .otp-code {{ font-size: 36px; letter-spacing: 8px; }}
            .greeting {{ font-size: 20px; }}
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <h1 class='logo'>🚀 {companyName}</h1>
        </div>
        <div class='content'>
            <div class='greeting'>Hey there! 👋</div>
            <p class='message'>
                Use the code below to complete your continue!
            </p>
            
            <div class='otp-container'>
                <div class='otp-label'>Your Verification Code</div>
                <div class='otp-code'>{otp}</div>
                <div class='timer-badge'>⏱️ Expires in 3 minutes</div>
            </div>

            <div class='security-notice'>
                <h3>Security First!</h3>
                <ul>
                    <li><strong>Never share</strong> this code with anyone, including our support team</li>
                    <li>This code works only <strong>once</strong> and expires in 3 minutes</li>
                    <li>Didn't request this? <strong>Ignore this email</strong> - your account is safe</li>
                </ul>
            </div>

            <div class='help-section'>
                <p><strong>Having trouble?</strong></p>
                <p>If you didn't request this code or need assistance, please contact our support team.</p>
            </div>
        </div>
        <div class='footer'>
            <p><strong>{companyName}</strong> - Powering Your Financial Future</p>
            <div class='footer-links'>
                <a href='#'>Privacy Policy</a> | 
                <a href='#'>Terms of Service</a> | 
                <a href='#'>Contact Support</a>
            </div>
            <p>&copy; {DateTime.UtcNow.Year} {companyName}. All rights reserved.</p>
            <p style='margin-top: 15px; font-size: 11px;'>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
    }
    
    public async Task<bool> SendOfferLetterEmailAsync(OfferLetterDto offerLetter)
    {
        var subject = $"Loan Offer Letter - {offerLetter.CompanyName}";
        var htmlBody = GenerateOfferLetterEmailTemplate(offerLetter);
        var pdfBytes = GenerateOfferLetterPdf(offerLetter);
        var fileName = $"Offer_Letter_{offerLetter.LoanId}.pdf";
        
        // Create a simple email body that references the attachment
        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; background-color: #ffffff; }}
        .cta-button {{ display: inline-block; background-color: #28a745; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{offerLetter.CompanyName}</h1>
            <p>Loan Offer Letter</p>
        </div>
        <div class='content'>
            <p>Dear {offerLetter.BorrowerName},</p>
            <p>Congratulations! Your loan application has been approved. Please find your offer letter attached to this email as a PDF document.</p>
            <p><strong>Next Steps:</strong></p>
            <ol>
                <li>Download and review the attached offer letter carefully</li>
                <li>Sign the offer letter (digitally or by hand)</li>
                <li>Upload the signed copy using the link below</li>
            </ol>
            <div style='text-align: center; margin: 30px 0;'>
                <a href='https://lendingdevweb.vercel.app/upload-offer-letter?loan_id={offerLetter.LoanId}' class='cta-button'>Upload Signed Offer Letter</a>
            </div>
            <p>If you have any questions, please don't hesitate to contact our support team.</p>
            <p>Best regards,<br/>{offerLetter.CompanyName}</p>
        </div>
    </div>
</body>
</html>";
        
        return await SendEmailAsync(offerLetter.BorrowerEmail, subject, emailBody, attachmentBytes: pdfBytes, attachmentFileName: fileName);
    }
    
    public async Task<bool> SendDisbursementNotificationAsync(string emailAddress, string borrowerName, decimal amount, string disbursementReference)
    {
        var subject = "Loan Disbursement Notification";
        var body = GenerateDisbursementEmailTemplate(borrowerName, amount, disbursementReference);
        
        return await SendEmailAsync(emailAddress, subject, body);
    }
    
    public async Task<bool> SendLoanApplicationSummaryEmailAsync(string emailAddress, string borrowerName, decimal loanAmount, int tenor, decimal monthlyRepayment, decimal totalRepayment, string productName, string companyName)
    {
        var subject = "Loan Application Submitted Successfully! 🎉";
        var body = GenerateLoanApplicationSummaryTemplate(borrowerName, loanAmount, tenor, monthlyRepayment, totalRepayment, productName, companyName);
        
        return await SendEmailAsync(emailAddress, subject, body);
    }

    public async Task<bool> SendImageReuploadRequestAsync(string emailAddress, string borrowerName, string reason, string reuploadUrl)
    {
        var subject = "Action Required: Please Re-upload Your ID Documents";
        var body = GenerateImageReuploadRequestTemplate(borrowerName, reason, reuploadUrl);
        
        return await SendEmailAsync(emailAddress, subject, body);
    }
    
    private string GenerateOfferLetterEmailTemplate(OfferLetterDto offer)
    {
        var frontendBaseUrl = _configuration["FrontendBaseUrl"] ?? "https://lendingdevweb.vercel.app";
        var uploadUrl = $"{frontendBaseUrl}/upload-offer-letter?loan_id={offer.LoanId}";
        
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
                    <strong>Please sign this document and upload the signed copy to complete your loan application.</strong>
                </p>
                
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='{uploadUrl}' class='cta-button' style='display: inline-block; background-color: #28a745; color: white; padding: 15px 40px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px;'>
                        📤 Upload Signed Offer Letter
                    </a>
                    <p style='margin-top: 15px; color: #666; font-size: 14px;'>Click the button above or copy this link:<br/><a href='{uploadUrl}' style='color: #007bff;'>{uploadUrl}</a></p>
                </div>
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
    private string GenerateLoanApplicationSummaryTemplate(string borrowerName, decimal loanAmount, int tenor, decimal monthlyRepayment, decimal totalRepayment, string productName, string companyName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6; 
            color: #1a1a1a; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 20px;
        }}
        .email-wrapper {{ 
            max-width: 650px; 
            margin: 0 auto; 
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .header {{ 
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            padding: 50px 40px;
            text-align: center;
            position: relative;
        }}
        .header::after {{
            content: '';
            position: absolute;
            bottom: -2px;
            left: 0;
            right: 0;
            height: 4px;
            background: linear-gradient(90deg, #34d399 0%, #10b981 100%);
        }}
        .success-icon {{
            font-size: 64px;
            margin-bottom: 15px;
        }}
        .header h1 {{ 
            color: white; 
            font-size: 32px; 
            font-weight: 700;
            margin-bottom: 10px;
        }}
        .header p {{
            color: rgba(255,255,255,0.9);
            font-size: 18px;
        }}
        .content {{ 
            padding: 50px 40px;
            background: white;
        }}
        .greeting {{ 
            font-size: 24px; 
            font-weight: 600; 
            color: #2d3748;
            margin-bottom: 20px;
        }}
        .intro-text {{ 
            font-size: 16px; 
            color: #4a5568;
            margin-bottom: 35px;
            line-height: 1.8;
        }}
        .summary-card {{
            background: linear-gradient(135deg, #f6f8fb 0%, #e9ecef 100%);
            border-radius: 12px;
            padding: 35px;
            margin: 30px 0;
            border: 2px solid #e2e8f0;
        }}
        .summary-title {{
            font-size: 18px;
            font-weight: 700;
            color: #2d3748;
            margin-bottom: 25px;
            text-align: center;
            text-transform: uppercase;
            letter-spacing: 1px;
        }}
        .detail-row {{
            display: flex;
            justify-content: space-between;
            padding: 15px 0;
            border-bottom: 1px solid #e2e8f0;
        }}
        .detail-row:last-child {{
            border-bottom: none;
        }}
        .detail-label {{
            font-size: 15px;
            color: #718096;
            font-weight: 500;
        }}
        .detail-value {{
            font-size: 15px;
            color: #2d3748;
            font-weight: 700;
        }}
        .highlight-amount {{
            background: linear-gradient(135deg, #10b981 0%, #059669 100%);
            color: white;
            padding: 20px;
            border-radius: 8px;
            margin: 25px 0;
            text-align: center;
        }}
        .highlight-amount .label {{
            font-size: 14px;
            opacity: 0.9;
            margin-bottom: 8px;
        }}
        .highlight-amount .amount {{
            font-size: 36px;
            font-weight: 800;
            letter-spacing: 1px;
        }}
        .info-box {{
            background: #eff6ff;
            border-left: 4px solid #3b82f6;
            padding: 20px;
            border-radius: 8px;
            margin: 30px 0;
        }}
        .info-box h3 {{
            color: #1e40af;
            font-size: 16px;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
        }}
        .info-box h3::before {{
            content: 'ℹ️';
            margin-right: 8px;
            font-size: 20px;
        }}
        .info-box ul {{
            margin: 0;
            padding-left: 20px;
            color: #1e3a8a;
        }}
        .info-box li {{
            margin: 8px 0;
            font-size: 14px;
        }}
        .next-steps {{
            background: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 20px;
            border-radius: 8px;
            margin: 30px 0;
        }}
        .next-steps h3 {{
            color: #92400e;
            font-size: 16px;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
        }}
        .next-steps h3::before {{
            content: '📋';
            margin-right: 8px;
            font-size: 20px;
        }}
        .next-steps ol {{
            margin: 0;
            padding-left: 20px;
            color: #78350f;
        }}
        .next-steps li {{
            margin: 10px 0;
            font-size: 14px;
            line-height: 1.6;
        }}
        .footer {{ 
            background: #2d3748;
            padding: 30px;
            text-align: center;
            color: #a0aec0;
        }}
        .footer p {{
            margin: 8px 0;
            font-size: 13px;
        }}
        .footer strong {{
            color: #cbd5e0;
        }}
        @media only screen and (max-width: 600px) {{
            .content {{ padding: 30px 20px; }}
            .summary-card {{ padding: 25px 20px; }}
            .greeting {{ font-size: 20px; }}
            .highlight-amount .amount {{ font-size: 28px; }}
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <div class='success-icon'>🎉</div>
            <h1>Application Submitted!</h1>
            <p>Your loan request is being reviewed</p>
        </div>
        <div class='content'>
            <div class='greeting'>Hello {borrowerName}! 👋</div>
            <p class='intro-text'>
                Thank you for submitting your loan application with <strong>{companyName}</strong>. 
                We're excited to help you achieve your financial goals! Your application has been received 
                and is now under review by our team.
            </p>
            
            <div class='summary-card'>
                <div class='summary-title'>📄 Application Summary</div>
                
                <div class='detail-row'>
                    <span class='detail-label'>Loan Product</span>
                    <span class='detail-value'>{productName}</span>
                </div>
                
                <div class='detail-row'>
                    <span class='detail-label'>Loan Amount</span>
                    <span class='detail-value'>₦{loanAmount:N2}</span>
                </div>
                
                <div class='detail-row'>
                    <span class='detail-label'>Loan Duration</span>
                    <span class='detail-value'>{tenor} months</span>
                </div>
                
                <div class='detail-row'>
                    <span class='detail-label'>Total Repayment</span>
                    <span class='detail-value'>₦{totalRepayment:N2}</span>
                </div>
            </div>

            <div class='highlight-amount'>
                <div class='label'>Your Monthly Payment</div>
                <div class='amount'>₦{monthlyRepayment:N2}</div>
            </div>

            <div class='next-steps'>
                <h3>What Happens Next?</h3>
                <ol>
                    <li><strong>Application Review:</strong> Our team will review your application and supporting documents within 24-48 hours</li>
                    <li><strong>Decision Notification:</strong> You'll receive an email notification once a decision has been made</li>
                    <li><strong>Approval & Disbursement:</strong> If approved, funds will be disbursed to your registered bank account</li>
                    <li><strong>Repayment Schedule:</strong> Your repayment will be automatically deducted from your salary account monthly</li>
                </ol>
            </div>

            <div class='info-box'>
                <h3>Important Information</h3>
                <ul>
                    <li>Reference Number: Your application ID can be used to track your status</li>
                    <li>Keep your contact details updated to receive timely notifications</li>
                    <li>Ensure your salary account has sufficient funds for repayments</li>
                    <li>Contact support if you have any questions about your application</li>
                </ul>
            </div>

            <p style='margin-top: 30px; font-size: 15px; color: #4a5568;'>
                We appreciate your trust in <strong>{companyName}</strong> and look forward to serving you!
            </p>
        </div>
        <div class='footer'>
            <p><strong>{companyName}</strong> - Powering Your Financial Future</p>
            <p style='margin-top: 15px;'>This is an automated message. Please do not reply to this email.</p>
            <p>&copy; {DateTime.UtcNow.Year} {companyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }

    private byte[] GenerateOfferLetterPdf(OfferLetterDto offer)
    {
        try
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Background(Colors.Blue.Medium).Padding(20).Column(column =>
                    {
                        column.Item().Text(offer.CompanyName)
                            .FontSize(24).Bold().FontColor(Colors.White);
                        column.Item().Text("Loan Offer Letter")
                            .FontSize(14).FontColor(Colors.White);
                    });

                    // Content
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(12);

                        // Title
                        column.Item().AlignCenter().Text("LOAN OFFER LETTER")
                            .FontSize(18).Bold().FontColor(Colors.Blue.Medium);

                        column.Item().LineHorizontal(1).LineColor(Colors.Blue.Medium);

                        // Date and Reference
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text(text =>
                            {
                                text.Span("Date: ").Bold();
                                text.Span(offer.OfferDate.ToString("MMMM dd, yyyy"));
                            });
                            row.RelativeItem().AlignRight().Text(text =>
                            {
                                text.Span("Reference: ").Bold();
                                text.Span($"LOL-{offer.LoanId.ToString()[..8].ToUpper()}");
                            });
                        });

                        // Borrower Details Section
                        column.Item().PaddingTop(10).Column(section =>
                        {
                            section.Item().Text("BORROWER DETAILS")
                                .FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                            section.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            
                            section.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(120);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Name:").SemiBold();
                                table.Cell().Text(offer.BorrowerName);
                                
                                table.Cell().Text("Email:").SemiBold();
                                table.Cell().Text(offer.BorrowerEmail);
                                
                                table.Cell().Text("Address:").SemiBold();
                                table.Cell().Text(offer.BorrowerAddress);
                            });
                        });

                        // Loan Details Section
                        column.Item().PaddingTop(15).Column(section =>
                        {
                            section.Item().Text("LOAN DETAILS")
                                .FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                            section.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            
                            // Highlighted Amount
                            section.Item().PaddingVertical(10).AlignCenter()
                                .Background(Colors.Green.Lighten4)
                                .Border(1).BorderColor(Colors.Green.Medium)
                                .Padding(15)
                                .Text($"₦{offer.LoanAmount:N2}")
                                .FontSize(22).Bold().FontColor(Colors.Green.Darken2);

                            section.Item().PaddingTop(10).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(150);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Loan Product:").SemiBold();
                                table.Cell().Text(offer.ProductName);
                                
                                table.Cell().Text("Purpose:").SemiBold();
                                table.Cell().Text(offer.Purpose);
                                
                                table.Cell().Text("Loan Tenure:").SemiBold();
                                table.Cell().Text($"{offer.DurationInMonths} Month(s)");
                                
                                table.Cell().Text("Interest Rate:").SemiBold();
                                table.Cell().Text($"{offer.InterestRate}% ({offer.InterestComputationBasis})");
                                
                                table.Cell().Text("Total Interest:").SemiBold();
                                table.Cell().Text($"₦{offer.TotalInterest:N2}");
                                
                                table.Cell().Text("Total Repayment:").SemiBold();
                                table.Cell().Text($"₦{offer.TotalRepayment:N2}").Bold();
                                
                                table.Cell().Text("Monthly Repayment:").SemiBold();
                                table.Cell().Text($"₦{offer.MonthlyRepayment:N2}").Bold();
                            });
                        });

                        // Important Dates Section
                        column.Item().PaddingTop(15).Column(section =>
                        {
                            section.Item().Text("IMPORTANT DATES")
                                .FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                            section.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            
                            section.Item().PaddingTop(5).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(180);
                                    columns.RelativeColumn();
                                });

                                table.Cell().Text("Offer Valid Until:").SemiBold();
                                table.Cell().Text(offer.ExpiryDate.ToString("MMMM dd, yyyy"));
                                
                                table.Cell().Text("Expected Disbursement:").SemiBold();
                                table.Cell().Text(offer.ExpectedDisbursementDate.ToString("MMMM dd, yyyy"));
                                
                                table.Cell().Text("Maturity Date:").SemiBold();
                                table.Cell().Text(offer.ExpectedMaturityDate.ToString("MMMM dd, yyyy"));
                                
                                table.Cell().Text("Grace Period:").SemiBold();
                                table.Cell().Text($"{offer.MoratoriumDays} days");
                            });
                        });

                        // Penalty Notice
                        column.Item().PaddingTop(10)
                            .Background(Colors.Grey.Lighten3)
                            .Border(1).BorderColor(Colors.Blue.Medium)
                            .Padding(10)
                            .Text(text =>
                            {
                                text.Span("Penalty for Default: ").Bold();
                                text.Span($"A penalty rate of {offer.PenaltyRate}% will be applied on outstanding principal for late payments.");
                            });

                        // Terms and Conditions
                        column.Item().PageBreak();
                        column.Item().Text("TERMS AND CONDITIONS")
                            .FontSize(12).Bold().FontColor(Colors.Blue.Medium);
                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        var terms = new[]
                        {
                            "This offer is valid for 7 days from the date of issue.",
                            "The borrower agrees to repay the loan amount plus interest according to the repayment schedule.",
                            "Late payments will attract a penalty as specified above.",
                            "The borrower authorizes deduction of loan repayments from their salary account.",
                            "The borrower confirms that all information provided is accurate and complete.",
                            "This loan is subject to the successful setup of a direct debit mandate.",
                            "The lender reserves the right to modify the terms with prior notice.",
                            "This agreement is governed by the laws of the Federal Republic of Nigeria."
                        };

                        for (int i = 0; i < terms.Length; i++)
                        {
                            column.Item().PaddingTop(3).Row(row =>
                            {
                                row.ConstantItem(20).Text($"{i + 1}.");
                                row.RelativeItem().Text(terms[i]);
                            });
                        }

                        // Signature Section
                        column.Item().PaddingTop(20).Column(section =>
                        {
                            section.Item().Text("ACCEPTANCE OF OFFER")
                                .FontSize(12).Bold();
                            
                            section.Item().PaddingTop(5).Text(
                                $"I, {offer.BorrowerName}, hereby accept the loan offer as detailed above and agree to abide by all terms and conditions.");
                            
                            section.Item().PaddingTop(15)
                                .Border(2).BorderColor(Colors.Grey.Medium)
                                .Background(Colors.Grey.Lighten4)
                                .Padding(30)
                                .Column(sigBox =>
                                {
                                    sigBox.Item().AlignCenter().Text("BORROWER'S SIGNATURE")
                                        .FontSize(11).Bold();
                                    sigBox.Item().PaddingTop(40).AlignCenter().Text("_________________________________");
                                    sigBox.Item().PaddingTop(5).AlignCenter().Text("Signature & Date")
                                        .FontSize(9);
                                });

                            section.Item().PaddingTop(10).AlignCenter().Text(
                                "Please sign this document and upload the signed copy to complete your loan application.")
                                .FontSize(9).Bold();
                        });

                        // Warning
                        column.Item().PaddingTop(10)
                            .Background(Colors.Orange.Lighten4)
                            .Border(1).BorderColor(Colors.Orange.Medium)
                            .Padding(10)
                            .Text(text =>
                            {
                                text.Span("⚠️ Important: ").Bold();
                                text.Span("Please review all terms carefully before signing. By signing this document, you are entering into a legally binding agreement.");
                            });
                    });

                    // Footer
                    page.Footer().Background(Colors.Grey.Lighten3).Padding(10).Column(footer =>
                    {
                        footer.Item().AlignCenter().Text($"This is an official loan offer from {offer.CompanyName}")
                            .FontSize(9);
                        footer.Item().AlignCenter().Text(offer.CompanyAddress)
                            .FontSize(8);
                        footer.Item().AlignCenter().Text($"© {DateTime.UtcNow.Year} {offer.CompanyName}. All rights reserved.")
                            .FontSize(8);
                    });
                });
            }).GeneratePdf();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF");
            throw;
        }
    }

    private string GenerateImageReuploadRequestTemplate(string borrowerName, string reason, string reuploadUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6; 
            color: #1a1a1a; 
            background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
            padding: 40px 20px;
        }}
        .email-wrapper {{ 
            max-width: 600px; 
            margin: 0 auto; 
            background: white;
            border-radius: 16px;
            overflow: hidden;
            box-shadow: 0 20px 60px rgba(0,0,0,0.3);
        }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 50px 40px;
            text-align: center;
        }}
        .header h1 {{ 
            font-size: 32px; 
            margin-bottom: 10px;
            font-weight: 700;
        }}
        .header p {{
            font-size: 16px;
            opacity: 0.95;
            margin: 0;
        }}
        .content {{ 
            padding: 50px 40px;
        }}
        .greeting {{
            font-size: 22px;
            font-weight: 600;
            color: #1a1a1a;
            margin-bottom: 25px;
        }}
        .message {{
            font-size: 16px;
            color: #4a5568;
            margin-bottom: 30px;
            line-height: 1.8;
        }}
        .reason-box {{
            background: linear-gradient(135deg, #fff5f5 0%, #fed7d7 100%);
            border-left: 4px solid #f56565;
            padding: 25px;
            border-radius: 8px;
            margin: 30px 0;
        }}
        .reason-title {{
            font-size: 14px;
            text-transform: uppercase;
            font-weight: 700;
            color: #c53030;
            letter-spacing: 0.5px;
            margin-bottom: 10px;
        }}
        .reason-text {{
            font-size: 16px;
            color: #2d3748;
            line-height: 1.6;
            font-weight: 500;
        }}
        .cta-container {{
            text-align: center;
            margin: 40px 0;
        }}
        .cta-button {{
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 18px 45px;
            text-decoration: none;
            border-radius: 50px;
            font-weight: 700;
            font-size: 16px;
            box-shadow: 0 10px 30px rgba(102, 126, 234, 0.4);
            transition: all 0.3s ease;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }}
        .info-box {{
            background: #f7fafc;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 25px;
            margin: 30px 0;
        }}
        .info-box h3 {{
            color: #2d3748;
            font-size: 18px;
            margin-bottom: 15px;
            font-weight: 600;
        }}
        .info-box ul {{
            list-style: none;
            padding: 0;
        }}
        .info-box li {{
            padding: 8px 0;
            padding-left: 25px;
            position: relative;
            color: #4a5568;
            font-size: 15px;
        }}
        .info-box li:before {{
            content: '✓';
            position: absolute;
            left: 0;
            color: #48bb78;
            font-weight: bold;
            font-size: 18px;
        }}
        .footer {{ 
            background: linear-gradient(135deg, #2d3748 0%, #1a202c 100%);
            color: white;
            padding: 40px;
            text-align: center;
        }}
        .footer p {{
            margin: 10px 0;
            opacity: 0.9;
        }}
        .divider {{
            height: 1px;
            background: linear-gradient(90deg, transparent, #cbd5e0, transparent);
            margin: 30px 0;
        }}
    </style>
</head>
<body>
    <div class='email-wrapper'>
        <div class='header'>
            <h1>📸 Document Update Required</h1>
            <p>Action needed on your loan application</p>
        </div>
        
        <div class='content'>
            <div class='greeting'>
                Hello {borrowerName},
            </div>
            
            <div class='message'>
                We're reviewing your loan application and need you to re-upload your ID documents to continue processing.
            </div>

            <div class='reason-box'>
                <div class='reason-title'>Reason for Re-upload</div>
                <div class='reason-text'>{reason}</div>
            </div>

            <div class='message'>
                Please click the button below to upload new, clear images of your identification documents. This will help us expedite your application review.
            </div>

            <div class='cta-container'>
                <a href='{reuploadUrl}' class='cta-button'>Upload New Documents</a>
            </div>

            <div class='divider'></div>

            <div class='info-box'>
                <h3>📋 Document Requirements</h3>
                <ul>
                    <li><strong>Clear & Readable:</strong> Ensure all text and photos are clearly visible</li>
                    <li><strong>Good Lighting:</strong> Take photos in well-lit conditions</li>
                    <li><strong>Complete Document:</strong> Include both front and back of your ID</li>
                    <li><strong>Valid Format:</strong> Upload in JPG, JPEG, or PNG format</li>
                    <li><strong>Recent:</strong> Documents should be current and not expired</li>
                </ul>
            </div>

            <div class='message' style='margin-top: 30px;'>
                If you have any questions or need assistance, please don't hesitate to reach out to our support team.
            </div>

            <div class='message' style='font-weight: 600; color: #2d3748;'>
                Thank you for your prompt attention to this matter!
            </div>
        </div>
        
        <div class='footer'>
            <p style='font-weight: 700; font-size: 16px;'>Lending Solution Support Team</p>
            <p style='margin-top: 15px;'>This is an automated message regarding your loan application.</p>
            <p>&copy; {DateTime.UtcNow.Year} Lending Solution. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
    }
}
