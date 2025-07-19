using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// SMS service implementation using Twilio (or other SMS provider)
/// </summary>
public class SmsService : ISmsService
{
    private readonly SmsSettings _smsSettings;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IOptions<SmsSettings> smsSettings, ILogger<SmsService> logger)
    {
        _smsSettings = smsSettings.Value;
        _logger = logger;
    }

    public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otp, string purpose = "BVN Verification")
    {
        var message = $"Your {purpose} code is: {otp}. Valid for 10 minutes. Do not share with anyone. - LendingSolution";
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            // Check if SMS service is properly configured
            if (string.IsNullOrEmpty(_smsSettings.ApiKey) || 
                _smsSettings.ApiKey.Contains("dummy") ||
                string.IsNullOrEmpty(_smsSettings.SenderId))
            {
                _logger.LogWarning("SMS service not configured - simulating SMS send to {PhoneNumber}", phoneNumber);
                _logger.LogInformation("SMS SIMULATION - To: {PhoneNumber}, Message: {Message}", phoneNumber, message);
                return true; // Simulate successful send
            }

            // TODO: Implement actual SMS sending using Twilio, Termii, or other SMS provider
            // Example for Twilio:
            /*
            var client = new TwilioRestClient(_smsSettings.AccountSid, _smsSettings.AuthToken);
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(_smsSettings.SenderId),
                to: new PhoneNumber(phoneNumber),
                client: client
            );
            */

            // For now, simulate sending
            _logger.LogInformation("SMS would be sent to {PhoneNumber}: {Message}", phoneNumber, message);
            await Task.Delay(100); // Simulate API call delay
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }
}
