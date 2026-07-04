using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Orchestrates notification delivery across email and SMS with automatic wallet management
/// </summary>
public class NotificationOrchestrationService : INotificationOrchestrationService
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly IWalletService _walletService;
    private readonly ISettingsService _settingsService;
    private readonly IWalletRepository _walletRepository;
    private readonly ILogger<NotificationOrchestrationService> _logger;

    public NotificationOrchestrationService(
        IEmailService emailService,
        ISmsService smsService,
        IWalletService walletService,
        ISettingsService settingsService,
        IWalletRepository walletRepository,
        ILogger<NotificationOrchestrationService> logger)
    {
        _emailService = emailService;
        _smsService = smsService;
        _walletService = walletService;
        _settingsService = settingsService;
        _walletRepository = walletRepository;
        _logger = logger;
    }

    public async Task<SendNotificationResult> SendNotificationAsync(SendNotificationRequest request)
    {
        var result = new SendNotificationResult();
        
        // Get fee settings - skip fees for system/admin operations (no company context)
        var isSystemOperation = request.CompanyId == Guid.Empty;
        var settings = await _settingsService.GetSettingsAsync();
        var otpFee = isSystemOperation ? 0m : settings.OtpFee;

        // Determine which channels are required
        var (emailRequired, smsRequired, emailPrimary, smsPrimary) = DetermineChannelRequirements(request.Channel);

        // Generate subject if not provided
        var subject = request.Subject ?? GenerateSubject(request.Type);
        var purpose = GeneratePurpose(request.Type);

        try
        {
            // PRIMARY CHANNEL HANDLING
            if (emailPrimary || (emailRequired && !smsPrimary))
            {
                await HandleEmailChannelAsync(request, result, otpFee, subject, purpose, isPrimary: true);
            }
            else if (smsPrimary || (smsRequired && !emailPrimary))
            {
                await HandleSmsChannelAsync(request, result, otpFee, purpose, isPrimary: true);
            }

            // SECONDARY CHANNEL HANDLING (if applicable)
            if (emailPrimary && smsRequired)
            {
                // Email was primary, try SMS as secondary
                await HandleSmsChannelAsync(request, result, otpFee, purpose, isPrimary: false);
            }
            else if (smsPrimary && emailRequired)
            {
                // SMS was primary, try Email as secondary
                await HandleEmailChannelAsync(request, result, otpFee, subject, purpose, isPrimary: false);
            }

            // Calculate total
            result.TotalFeeDeducted = result.EmailFeeDeducted + result.SmsFeeDeducted;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in notification orchestration for {NotificationType}", request.Type);
            throw;
        }
    }

    public async Task<SendNotificationResult> SendOtpAsync(
        Guid companyId,
        string? email,
        string? phoneNumber,
        string otp,
        NotificationType type,
        NotificationChannel preferredChannel,
        string? senderName = null)
    {
        var request = new SendNotificationRequest
        {
            CompanyId = companyId,
            EmailAddress = email,
            PhoneNumber = phoneNumber,
            Content = otp,
            Type = type,
            Channel = preferredChannel,
            SenderName = senderName,
            ThrowOnPrimaryFailure = true,
            ThrowOnSecondaryFailure = false
        };

        return await SendNotificationAsync(request);
    }

    private async Task HandleEmailChannelAsync(
        SendNotificationRequest request,
        SendNotificationResult result,
        decimal otpFee,
        string subject,
        string purpose,
        bool isPrimary)
    {
        var channelLabel = isPrimary ? "Primary Email" : "Secondary Email";
        
        try
        {
            // Validate wallet balance
            if (isPrimary || request.Channel == NotificationChannel.Both)
            {
                await _walletService.ValidateCompanyBalanceForFeeAsync(
                    request.CompanyId,
                    otpFee,
                    $"{channelLabel} - {purpose}");
            }
            else
            {
                // For secondary, check balance without throwing
                var wallet = await _walletService.GetWalletByCompanyIdAsync(request.CompanyId);
                if (wallet == null || wallet.Balance < otpFee)
                {
                    result.Warnings.Add($"Insufficient balance for {channelLabel}. Email not sent.");
                    _logger.LogWarning("Insufficient balance for {ChannelLabel} for company {CompanyId}", 
                        channelLabel, request.CompanyId);
                    return;
                }
            }

            // Send email
            bool emailSent;
            if (request.Type == NotificationType.EmailVerificationOTP || 
                request.Type == NotificationType.BvnVerificationOTP)
            {
                emailSent = await _emailService.SendOtpEmailAsync(
                    request.EmailAddress!,
                    request.OtpCode ?? request.Content,
                    purpose,
                    request.SenderName);
            }
            else
            {
                emailSent = await _emailService.SendEmailAsync(
                    request.EmailAddress!,
                    subject,
                    request.Content,
                    request.SenderName,
                    null,
                    request.AttachmentBytes,
                    request.AttachmentFileName);
            }

            if (!emailSent)
            {
                var errorMsg = $"Failed to send {channelLabel}";
                if (isPrimary && request.ThrowOnPrimaryFailure)
                {
                    throw new AppException(errorMsg, 500);
                }
                else if (!isPrimary && request.ThrowOnSecondaryFailure)
                {
                    throw new AppException(errorMsg, 500);
                }
                else
                {
                    result.Warnings.Add(errorMsg);
                    _logger.LogWarning("{ErrorMsg} for company {CompanyId}", errorMsg, request.CompanyId);
                    return;
                }
            }

            // Deduct fee on success
            await DeductFeeAsync(request.CompanyId, otpFee, $"{channelLabel} - {purpose}");
            result.EmailSent = true;
            result.EmailFeeDeducted = otpFee;
            
            _logger.LogInformation("{ChannelLabel} sent successfully for {NotificationType}", 
                channelLabel, request.Type);
        }
        catch (AppException ex) when (ex.StatusCode == 400 || ex.StatusCode == 404)
        {
            // Wallet validation failed
            if (isPrimary)
            {
                _logger.LogError("Wallet validation failed for {ChannelLabel}: {Message}", channelLabel, ex.Message);
                throw new AppException("Unable to send notification. Please contact support.", 503);
            }
            else
            {
                result.Warnings.Add($"Wallet validation failed for {channelLabel}");
                _logger.LogWarning("Wallet validation failed for {ChannelLabel}: {Message}", channelLabel, ex.Message);
            }
        }
    }

    private async Task HandleSmsChannelAsync(
        SendNotificationRequest request,
        SendNotificationResult result,
        decimal otpFee,
        string purpose,
        bool isPrimary)
    {
        var channelLabel = isPrimary ? "Primary SMS" : "Secondary SMS";
        
        try
        {
            // Validate wallet balance
            if (isPrimary || request.Channel == NotificationChannel.Both)
            {
                await _walletService.ValidateCompanyBalanceForFeeAsync(
                    request.CompanyId,
                    otpFee,
                    $"{channelLabel} - {purpose}");
            }
            else
            {
                // For secondary, check balance without throwing
                var wallet = await _walletService.GetWalletByCompanyIdAsync(request.CompanyId);
                if (wallet == null || wallet.Balance < otpFee)
                {
                    result.Warnings.Add($"Insufficient balance for {channelLabel}. SMS not sent.");
                    _logger.LogWarning("Insufficient balance for {ChannelLabel} for company {CompanyId}", 
                        channelLabel, request.CompanyId);
                    return;
                }
            }

            // Send SMS
            var smsSent = await _smsService.SendOtpSmsAsync(
                request.PhoneNumber!,
                request.Content,
                purpose);

            if (!smsSent)
            {
                var errorMsg = $"Failed to send {channelLabel}";
                if (isPrimary && request.ThrowOnPrimaryFailure)
                {
                    throw new AppException(errorMsg, 500);
                }
                else if (!isPrimary && request.ThrowOnSecondaryFailure)
                {
                    throw new AppException(errorMsg, 500);
                }
                else
                {
                    result.Warnings.Add(errorMsg);
                    _logger.LogWarning("{ErrorMsg} for company {CompanyId}", errorMsg, request.CompanyId);
                    return;
                }
            }

            // Deduct fee on success
            await DeductFeeAsync(request.CompanyId, otpFee, $"{channelLabel} - {purpose}");
            result.SmsSent = true;
            result.SmsFeeDeducted = otpFee;
            
            _logger.LogInformation("{ChannelLabel} sent successfully for {NotificationType}", 
                channelLabel, request.Type);
        }
        catch (AppException ex) when (ex.StatusCode == 400 || ex.StatusCode == 404)
        {
            // Wallet validation failed
            if (isPrimary)
            {
                _logger.LogError("Wallet validation failed for {ChannelLabel}: {Message}", channelLabel, ex.Message);
                throw new AppException("Unable to send notification. Please contact support.", 503);
            }
            else
            {
                result.Warnings.Add($"Wallet validation failed for {channelLabel}");
                _logger.LogWarning("Wallet validation failed for {ChannelLabel}: {Message}", channelLabel, ex.Message);
            }
        }
    }

    private async Task DeductFeeAsync(Guid companyId, decimal amount, string description)
    {
        // Skip fee deduction for system/admin operations or zero-amount fees
        if (companyId == Guid.Empty || amount <= 0)
        {
            _logger.LogInformation("Skipping fee deduction: CompanyId={CompanyId}, Amount={Amount}", companyId, amount);
            return;
        }

        var companyWallet = await _walletRepository.GetWalletByCompanyIdAsync(companyId);
        if (companyWallet == null)
        {
            _logger.LogError("Company wallet not found for company {CompanyId} during fee deduction", companyId);
            return;
        }

        companyWallet.Balance -= amount;
        await _walletRepository.UpdateWalletAsync(companyWallet);

        _logger.LogInformation(
            "Fee deducted: {Amount:C} from Company {CompanyId} wallet for {Description}. New balance: {Balance:C}",
            amount, companyId, description, companyWallet.Balance);
    }

    private (bool emailRequired, bool smsRequired, bool emailPrimary, bool smsPrimary) 
        DetermineChannelRequirements(NotificationChannel channel)
    {
        return channel switch
        {
            NotificationChannel.Email => (true, false, true, false),
            NotificationChannel.SMS => (false, true, false, true),
            NotificationChannel.EmailPrimary => (true, true, true, false),
            NotificationChannel.SMSPrimary => (true, true, false, true),
            NotificationChannel.Both => (true, true, true, false), // Email first, both required
            _ => throw new ArgumentException($"Unknown notification channel: {channel}")
        };
    }

    private string GenerateSubject(NotificationType type)
    {
        return type switch
        {
            NotificationType.EmailVerificationOTP => "Your Email Verification Code",
            NotificationType.BvnVerificationOTP => "Your BVN Verification Code",
            NotificationType.OfferLetter => "Your Loan Offer Letter",
            NotificationType.DisbursementNotification => "Loan Disbursement Confirmation",
            NotificationType.LoanApplicationSummary => "Loan Application Summary",
            NotificationType.ImageReuploadRequest => "Document Upload Request",
            NotificationType.GeneralNotification => "Notification",
            _ => "Notification"
        };
    }

    private string GeneratePurpose(NotificationType type)
    {
        return type switch
        {
            NotificationType.EmailVerificationOTP => "Email Verification",
            NotificationType.BvnVerificationOTP => "BVN Verification",
            NotificationType.OfferLetter => "Offer Letter",
            NotificationType.DisbursementNotification => "Disbursement Notification",
            NotificationType.LoanApplicationSummary => "Loan Application",
            NotificationType.ImageReuploadRequest => "Document Upload",
            NotificationType.GeneralNotification => "Notification",
            _ => "Notification"
        };
    }
}
