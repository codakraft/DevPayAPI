using System.Security.Cryptography;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Service for managing OTP lifecycle with security features and audit trail
/// </summary>
public class OtpService : IOtpService
{
    private readonly IOtpRepository _otpRepository;
    private readonly INotificationOrchestrationService _notificationService;
    private readonly ILogger<OtpService> _logger;

    // Security configuration
    private const int MaxOtpRequestsPer5Minutes = 3;
    private const int MaxValidationAttempts = 5;
    private const int StandardOtpLength = 6;
    private const int SensitiveOtpLength = 8;
    private const int TransactionOtpExpiryMinutes = 3;
    private const int VerificationOtpExpiryMinutes = 10;

    public OtpService(
        IOtpRepository otpRepository,
        INotificationOrchestrationService notificationService,
        ILogger<OtpService> logger)
    {
        _otpRepository = otpRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<GenerateOtpResult> GenerateAndSendOtpAsync(GenerateOtpRequest request)
    {
        try
        {
            // 1. Rate Limiting Check
            var recentOtps = await _otpRepository.GetRecentOtpsAsync(
                request.RecipientIdentifier,
                request.Type,
                TimeSpan.FromMinutes(5));

            if (recentOtps.Count >= MaxOtpRequestsPer5Minutes)
            {
                _logger.LogWarning("Rate limit exceeded for {Recipient}, Type: {Type}. Count: {Count}",
                    request.RecipientIdentifier, request.Type, recentOtps.Count);

                return GenerateOtpResult.Fail("Too many OTP requests. Please wait before requesting another OTP.");
            }

            // 2. Invalidate any existing active OTPs for this recipient/type
            var existingOtp = await _otpRepository.GetActiveOtpAsync(request.RecipientIdentifier, request.Type);
            if (existingOtp != null)
            {
                existingOtp.IsInvalidated = true;
                existingOtp.InvalidationReason = "New OTP generated";
                existingOtp.UpdatedAt = DateTime.UtcNow;
                existingOtp.InvalidatedAt = DateTime.UtcNow;
                await _otpRepository.UpdateAsync(existingOtp);
                
                _logger.LogInformation("Invalidated existing OTP {OtpId} for new request", existingOtp.Id);
            }

            // 3. Generate OTP
            var otpLength = request.CodeLengthOverride ?? 
                           (IsSensitiveOperation(request.Type) ? SensitiveOtpLength : StandardOtpLength);
            var code = GenerateSecureOtp(otpLength);
            _logger.LogWarning("[DEV] OTP for {Recipient} ({Type}): {Code}", request.RecipientIdentifier, request.Type, code);
            var expiryMinutes = request.ExpiryMinutesOverride ??
                               (IsTransactionOperation(request.Type) ? TransactionOtpExpiryMinutes : VerificationOtpExpiryMinutes);

            // 4. Create OTP entity
            var otp = new Otp
            {
                Code = code,
                Type = request.Type,
                CodeLength = otpLength,
                RecipientIdentifier = request.RecipientIdentifier,
                Purpose = request.CustomPurpose ?? GetDefaultPurpose(request.Type),
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
                ExpiryMinutes = expiryMinutes,
                MaxAttempts = request.MaxAttemptsOverride ?? MaxValidationAttempts,
                AttemptCount = 0,
                IsUsed = false,
                IsLocked = false,
                IsInvalidated = false,
                DeliveryChannel = request.DeliveryChannel,
                CompanyId = request.CompanyId,
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                CreatedBy = request.CreatedBy
            };

            await _otpRepository.CreateAsync(otp);

            // 5. Send OTP via notification orchestration
            var notificationRequest = new SendNotificationRequest
            {
                CompanyId = request.CompanyId ?? Guid.Empty,
                EmailAddress = request.Type == OtpType.EmailVerification || request.DeliveryChannel != NotificationChannel.SMS 
                    ? request.RecipientIdentifier : null,
                PhoneNumber = request.Type == OtpType.PhoneVerification || request.DeliveryChannel != NotificationChannel.Email 
                    ? request.RecipientIdentifier : null,
                Channel = request.DeliveryChannel,
                Subject = GetOtpSubject(request.Type),
                Content = FormatOtpMessage(code, expiryMinutes, request.Type, request.SenderName),
                OtpCode = code
            };

            var notificationResult = await _notificationService.SendNotificationAsync(notificationRequest);

            // 6. Update OTP delivery status
            otp.WasDelivered = notificationResult.EmailSent || notificationResult.SmsSent;
            otp.DeliveredAt = otp.WasDelivered ? DateTime.UtcNow : null;
            otp.DeliveryError = !otp.WasDelivered ? "Notification delivery failed" : null;
            await _otpRepository.UpdateAsync(otp);

            if (!otp.WasDelivered)
            {
                _logger.LogWarning("OTP {OtpId} generated but delivery failed", otp.Id);
                return GenerateOtpResult.Fail("Failed to send OTP. Please try again.");
            }

            _logger.LogInformation("OTP {OtpId} generated and sent successfully via {Channel}", 
                otp.Id, request.DeliveryChannel);

            return GenerateOtpResult.Succeed(otp.Id, otp.ExpiresAt, notificationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating OTP for {Recipient}, Type: {Type}", 
                request.RecipientIdentifier, request.Type);
            
            return GenerateOtpResult.Fail("An error occurred while generating OTP. Please try again.");
        }
    }

    public async Task<ValidateOtpResult> ValidateOtpAsync(ValidateOtpRequest request)
    {
        try
        {
            // 1. Get active OTP
            var otp = await _otpRepository.GetActiveOtpAsync(request.RecipientIdentifier, request.Type);

            if (otp == null)
            {
                _logger.LogWarning("No active OTP found for {Recipient}, Type: {Type}", 
                    request.RecipientIdentifier, request.Type);
                
                return ValidateOtpResult.Fail("Invalid or expired OTP. Please request a new one.");
            }

            // 2. Check if OTP is locked
            if (otp.IsLocked)
            {
                _logger.LogWarning("OTP {OtpId} is locked due to too many failed attempts", otp.Id);
                return ValidateOtpResult.Fail("This OTP has been locked due to too many failed attempts. Please request a new one.");
            }

            // 3. Check if OTP has expired
            if (otp.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogInformation("OTP {OtpId} has expired", otp.Id);
                return ValidateOtpResult.Fail("This OTP has expired. Please request a new one.");
            }

            // 4. Validate OTP code
            var isValid = otp.Code == request.Code;

            // 5. Update attempt count
            otp.AttemptCount++;
            otp.UpdatedAt = DateTime.UtcNow;

            if (isValid)
            {
                // Mark as used
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
                await _otpRepository.UpdateAsync(otp);

                _logger.LogInformation("OTP {OtpId} validated successfully", otp.Id);
                return ValidateOtpResult.Succeed(otp.Id);
            }
            else
            {
                // Check if we should lock the OTP
                if (otp.AttemptCount >= otp.MaxAttempts)
                {
                    otp.IsLocked = true;
                    otp.LockedAt = DateTime.UtcNow;
                    _logger.LogWarning("OTP {OtpId} locked after {Attempts} failed attempts", 
                        otp.Id, otp.AttemptCount);
                }

                await _otpRepository.UpdateAsync(otp);

                var remainingAttempts = otp.MaxAttempts - otp.AttemptCount;
                var message = otp.IsLocked
                    ? "Too many failed attempts. This OTP has been locked. Please request a new one."
                    : $"Invalid OTP. You have {remainingAttempts} attempt(s) remaining.";

                return ValidateOtpResult.Fail(message, remainingAttempts);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating OTP for {Recipient}, Type: {Type}", 
                request.RecipientIdentifier, request.Type);
            
            return ValidateOtpResult.Fail("An error occurred while validating OTP. Please try again.");
        }
    }

    public async Task<ResendOtpResult> ResendOtpAsync(ResendOtpRequest request)
    {
        try
        {
            var otp = await _otpRepository.GetActiveOtpAsync(request.RecipientIdentifier, request.Type);

            if (otp == null || otp.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogInformation("No active OTP to resend or expired. Generating new OTP for {Recipient}", 
                    request.RecipientIdentifier);

                // Generate new OTP
                var generateRequest = new GenerateOtpRequest
                {
                    Type = request.Type,
                    RecipientIdentifier = request.RecipientIdentifier,
                    CompanyId = request.CompanyId,
                    DeliveryChannel = request.DeliveryChannel ?? NotificationChannel.Email,
                    SenderName = request.SenderName
                };

                var result = await GenerateAndSendOtpAsync(generateRequest);
                
                return result.Success 
                    ? ResendOtpResult.Succeed(result.OtpId, true, result.ExpiresAt, result.NotificationResult!)
                    : ResendOtpResult.Fail(result.ErrorMessage ?? "Failed to generate new OTP");
            }

            // Resend existing OTP
            var notificationRequest = new SendNotificationRequest
            {
                CompanyId = request.CompanyId ?? Guid.Empty,
                EmailAddress = request.Type == OtpType.EmailVerification || request.DeliveryChannel != NotificationChannel.SMS 
                    ? request.RecipientIdentifier : null,
                PhoneNumber = request.Type == OtpType.PhoneVerification || request.DeliveryChannel != NotificationChannel.Email
                    ? request.RecipientIdentifier : null,
                Channel = request.DeliveryChannel ?? otp.DeliveryChannel,
                Subject = GetOtpSubject(request.Type),
                Content = FormatOtpMessage(otp.Code, (int)(otp.ExpiresAt - DateTime.UtcNow).TotalMinutes, request.Type, request.SenderName),
                // Without this the email template falls back to Content and keeps only its digits,
                // so the minutes left are appended to the code (e.g. 6-digit code + "9" minutes = 7 digits)
                OtpCode = otp.Code
            };

            var notificationResult = await _notificationService.SendNotificationAsync(notificationRequest);

            if (!notificationResult.EmailSent && !notificationResult.SmsSent)
            {
                return ResendOtpResult.Fail("Failed to resend OTP. Please try again.");
            }

            _logger.LogInformation("Resent existing OTP {OtpId} successfully", otp.Id);

            return ResendOtpResult.Succeed(otp.Id, false, otp.ExpiresAt, notificationResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending OTP for {Recipient}, Type: {Type}", 
                request.RecipientIdentifier, request.Type);
            
            return ResendOtpResult.Fail("An error occurred while resending OTP. Please try again.");
        }
    }

    public async Task<OtpStatusResult> GetOtpStatusAsync(string recipientIdentifier, OtpType type)
    {
        var otp = await _otpRepository.GetActiveOtpAsync(recipientIdentifier, type);

        if (otp == null)
        {
            return new OtpStatusResult
            {
                HasActiveOtp = false
            };
        }

        var isExpired = otp.ExpiresAt < DateTime.UtcNow;

        return new OtpStatusResult
        {
            HasActiveOtp = !isExpired && !otp.IsUsed && !otp.IsLocked,
            OtpId = otp.Id,
            IsLocked = otp.IsLocked,
            ExpiresAt = otp.ExpiresAt,
            RemainingAttempts = otp.MaxAttempts - otp.AttemptCount
        };
    }

    public async Task<bool> InvalidateOtpAsync(Guid otpId, string reason)
    {
        var otp = await _otpRepository.GetByIdAsync(otpId);

        if (otp == null)
        {
            _logger.LogWarning("Cannot invalidate OTP {OtpId} - not found", otpId);
            return false;
        }

        if (otp.IsUsed || otp.IsInvalidated)
        {
            _logger.LogInformation("OTP {OtpId} already used or invalidated", otpId);
            return true;
        }

        otp.IsInvalidated = true;
        otp.InvalidationReason = reason;
        otp.InvalidatedAt = DateTime.UtcNow;
        otp.UpdatedAt = DateTime.UtcNow;

        await _otpRepository.UpdateAsync(otp);

        _logger.LogInformation("OTP {OtpId} invalidated. Reason: {Reason}", otpId, reason);

        return true;
    }

    public async Task<int> CleanupExpiredOtpsAsync(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var expiredIds = await _otpRepository.GetExpiredOtpIdsAsync(cutoffDate);

        if (!expiredIds.Any())
        {
            _logger.LogInformation("No expired OTPs to clean up");
            return 0;
        }

        await _otpRepository.DeleteByIdsAsync(expiredIds);

        _logger.LogInformation("Cleaned up {Count} expired OTPs older than {Days} days", 
            expiredIds.Count, daysOld);

        return expiredIds.Count;
    }

    public async Task<OtpAnalytics> GetOtpAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? companyId = null)
    {
        var rawAnalytics = await _otpRepository.GetAnalyticsAsync(startDate, endDate, companyId);

        return new OtpAnalytics
        {
            TotalGenerated = (int)rawAnalytics["TotalGenerated"],
            TotalValidated = (int)rawAnalytics["TotalValidated"],
            TotalFailed = (int)rawAnalytics["TotalFailed"],
            TotalExpired = (int)rawAnalytics["TotalExpired"],
            TotalLocked = (int)rawAnalytics["TotalLocked"],
            SuccessRate = (decimal)rawAnalytics["SuccessRate"],
            AverageValidationTimeSeconds = Convert.ToDecimal((double)rawAnalytics["AverageValidationTimeSeconds"]),
            GenerationsByType = ConvertStringDictToOtpType((Dictionary<string, int>)rawAnalytics["GenerationsByType"]),
            DeliveriesByChannel = ConvertStringDictToChannel((Dictionary<string, int>)rawAnalytics["DeliveriesByChannel"])
        };
    }

    #region Private Helper Methods

    private string GenerateSecureOtp(int length)
    {
        // Use cryptographically secure random number generator
        var max = (int)Math.Pow(10, length);
        var number = RandomNumberGenerator.GetInt32(0, max);
        return number.ToString($"D{length}");
    }

    private bool IsSensitiveOperation(OtpType type)
    {
        return type == OtpType.TransactionAuthorization ||
               type == OtpType.AccountRecovery ||
               type == OtpType.DocumentSigning;
    }

    private bool IsTransactionOperation(OtpType type)
    {
        return type == OtpType.TransactionAuthorization;
    }

    private string GetDefaultPurpose(OtpType type)
    {
        return type switch
        {
            OtpType.EmailVerification => "Email Verification",
            OtpType.PhoneVerification => "Phone Verification",
            OtpType.BvnVerification => "BVN Verification",
            OtpType.TwoFactorAuthentication => "Two-Factor Authentication",
            OtpType.PasswordReset => "Password Reset",
            OtpType.TransactionAuthorization => "Transaction Authorization",
            OtpType.AccountRecovery => "Account Recovery",
            OtpType.DocumentSigning => "Document Signing",
            _ => "Verification"
        };
    }

    private string GetOtpSubject(OtpType type)
    {
        return type switch
        {
            OtpType.EmailVerification => "Email Verification Code",
            OtpType.PhoneVerification => "Phone Verification Code",
            OtpType.BvnVerification => "BVN Verification Code",
            OtpType.TwoFactorAuthentication => "Two-Factor Authentication Code",
            OtpType.PasswordReset => "Password Reset Code",
            OtpType.TransactionAuthorization => "Transaction Authorization Code",
            OtpType.AccountRecovery => "Account Recovery Code",
            OtpType.DocumentSigning => "Document Signing Code",
            _ => "Verification Code"
        };
    }

    private string FormatOtpMessage(string code, int expiryMinutes, OtpType type, string? senderName = null)
    {
        var purpose = type switch
        {
            OtpType.EmailVerification => "verify your email address",
            OtpType.PhoneVerification => "verify your phone number",
            OtpType.BvnVerification => "verify your BVN",
            OtpType.TwoFactorAuthentication => "complete your login",
            OtpType.PasswordReset => "reset your password",
            OtpType.TransactionAuthorization => "authorize this transaction",
            OtpType.AccountRecovery => "recover your account",
            OtpType.DocumentSigning => "sign this document",
            _ => "complete this action"
        };

        var greeting = !string.IsNullOrWhiteSpace(senderName) 
            ? $"Dear valued customer of {senderName},\n\n"
            : "";

        return $"{greeting}Your verification code is: {code}\n\n" +
               $"Use this code to {purpose}.\n\n" +
               $"This code will expire in {expiryMinutes} minutes.\n\n" +
               $"If you did not request this code, please ignore this message.";
    }

    private Dictionary<OtpType, int> ConvertStringDictToOtpType(Dictionary<string, int> dict)
    {
        return dict.ToDictionary(
            kvp => Enum.Parse<OtpType>(kvp.Key),
            kvp => kvp.Value
        );
    }

    private Dictionary<NotificationChannel, int> ConvertStringDictToChannel(Dictionary<string, int> dict)
    {
        return dict.ToDictionary(
            kvp => Enum.Parse<NotificationChannel>(kvp.Key),
            kvp => kvp.Value
        );
    }

    #endregion
}
