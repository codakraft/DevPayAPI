using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces;

public interface IMonoService
{
    // Mandate Management
    Task<MonoGenerateMandateResponseDto?> GenerateMandateAsync(Guid loanId, MonoGenerateMandateRequestDto request, string? userId = null);
    Task<MonoCancelMandateResponseDto?> CancelMandateAsync(string mandateId, string? userId = null);
    Task<MonoPauseMandateResponseDto?> PauseMandateAsync(string mandateId, string? userId = null);
    Task<MonoReinstateMandateResponseDto?> ReinstateMandateAsync(string mandateId, string? userId = null);
    
    // Data Services
    Task<MonoBanksResponseDto?> GetBanksAsync();
    
    // BVN Validation (Original)
    Task<MonoBvnLookupResponseDto?> BvnLookupAsync(MonoBvnLookupRequestDto request, string? userId = null);
    Task<MonoBvnVerifyResponseDto?> BvnVerifyAsync(MonoBvnVerifyRequestDto request, string? userId = null);
    Task<MonoBvnDetailsResponseDto?> BvnGetDetailsAsync(MonoBvnDetailsRequestDto request, string? userId = null);
    Task<MonoBvnValidationResultDto?> ValidateBvnCompleteAsync(string bvn, string method, string phoneNumber, string otp, string scope = "identity", string? userId = null);
    
    // Session-based BVN Validation (Enhanced)
    Task<MonoBvnVerifyResponseDto?> BvnVerifyWithSessionAsync(MonoBvnSessionRequestDto request, string? userId = null);
    Task<MonoBvnDetailsResponseDto?> BvnGetDetailsWithSessionAsync(MonoBvnSessionDetailsRequestDto request, string? userId = null);
    
    // BVN Verification Tracking
    Task<bool> IsBvnAlreadyVerifiedAsync(string bvn);
    Task<MonoBvnVerificationRecord?> GetBvnVerificationRecordAsync(string bvn);
    
    // Credit History
    Task<MonoCreditHistoryResponseDto?> GetCreditHistoryAsync(string bvn, string provider = "xds", string? userId = null);
    Task<MonoCreditAnalysisResultDto?> AnalyzeCreditHistoryAsync(string bvn, string provider = "xds", string? userId = null);
    
    // Creditworthiness Check
    Task<MonoCreditworthinessResponseDto?> CheckCreditworthinessAsync(MonoCreditworthinessRequestDto request, string? userId = null);
}