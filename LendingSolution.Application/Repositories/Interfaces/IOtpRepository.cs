using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

/// <summary>
/// Repository interface for OTP operations
/// </summary>
public interface IOtpRepository
{
    /// <summary>
    /// Create a new OTP
    /// </summary>
    Task<Otp> CreateAsync(Otp otp);
    
    /// <summary>
    /// Update an existing OTP
    /// </summary>
    Task<Otp> UpdateAsync(Otp otp);
    
    /// <summary>
    /// Get OTP by ID
    /// </summary>
    Task<Otp?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Get the most recent active OTP for a recipient and type
    /// </summary>
    /// <param name="recipientIdentifier">Recipient (email, phone, etc.)</param>
    /// <param name="type">OTP type</param>
    /// <returns>Active OTP if exists, otherwise null</returns>
    Task<Otp?> GetActiveOtpAsync(string recipientIdentifier, OtpType type);
    
    /// <summary>
    /// Get recent OTPs for rate limiting check
    /// </summary>
    /// <param name="recipientIdentifier">Recipient identifier</param>
    /// <param name="type">OTP type</param>
    /// <param name="timeWindow">Time window to check (e.g., last 5 minutes)</param>
    /// <returns>List of recent OTPs</returns>
    Task<List<Otp>> GetRecentOtpsAsync(string recipientIdentifier, OtpType type, TimeSpan timeWindow);
    
    /// <summary>
    /// Get all OTPs for a recipient (for audit/history)
    /// </summary>
    Task<List<Otp>> GetOtpHistoryAsync(string recipientIdentifier, OtpType? type = null, int limit = 50);
    
    /// <summary>
    /// Get expired OTPs for cleanup
    /// </summary>
    /// <param name="olderThan">Delete OTPs older than this date</param>
    /// <returns>List of expired OTP IDs</returns>
    Task<List<Guid>> GetExpiredOtpIdsAsync(DateTime olderThan);
    
    /// <summary>
    /// Delete OTPs by IDs (cleanup operation)
    /// </summary>
    Task DeleteByIdsAsync(List<Guid> ids);
    
    /// <summary>
    /// Get OTP analytics data
    /// </summary>
    Task<Dictionary<string, object>> GetAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? companyId = null);
    
    /// <summary>
    /// Get OTPs by company ID (for company-specific reporting)
    /// </summary>
    Task<List<Otp>> GetByCompanyIdAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null);
}
