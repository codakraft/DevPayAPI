using LendingSolution.Application.Services.Interfaces;
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
}
