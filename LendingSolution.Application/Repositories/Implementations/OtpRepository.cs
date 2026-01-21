using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Repositories.Implementations;

/// <summary>
/// Repository implementation for OTP operations
/// </summary>
public class OtpRepository : IOtpRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OtpRepository> _logger;

    public OtpRepository(ApplicationDbContext context, ILogger<OtpRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Otp> CreateAsync(Otp otp)
    {
        otp.CreatedAt = DateTime.UtcNow;
        otp.UpdatedAt = DateTime.UtcNow;
        
        _context.Otps.Add(otp);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("OTP created: {OtpId}, Type: {Type}, Recipient: {Recipient}", 
            otp.Id, otp.Type, otp.RecipientIdentifier);
        
        return otp;
    }

    public async Task<Otp> UpdateAsync(Otp otp)
    {
        otp.UpdatedAt = DateTime.UtcNow;
        
        _context.Otps.Update(otp);
        await _context.SaveChangesAsync();
        
        _logger.LogDebug("OTP updated: {OtpId}", otp.Id);
        
        return otp;
    }

    public async Task<Otp?> GetByIdAsync(Guid id)
    {
        return await _context.Otps
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Otp?> GetActiveOtpAsync(string recipientIdentifier, OtpType type)
    {
        var now = DateTime.UtcNow;
        
        return await _context.Otps
            .Where(o => o.RecipientIdentifier == recipientIdentifier)
            .Where(o => o.Type == type)
            .Where(o => !o.IsUsed)
            .Where(o => !o.IsInvalidated)
            .Where(o => o.ExpiresAt > now)
            .OrderByDescending(o => o.GeneratedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Otp>> GetRecentOtpsAsync(string recipientIdentifier, OtpType type, TimeSpan timeWindow)
    {
        var cutoffTime = DateTime.UtcNow - timeWindow;
        
        return await _context.Otps
            .Where(o => o.RecipientIdentifier == recipientIdentifier)
            .Where(o => o.Type == type)
            .Where(o => o.GeneratedAt >= cutoffTime)
            .OrderByDescending(o => o.GeneratedAt)
            .ToListAsync();
    }

    public async Task<List<Otp>> GetOtpHistoryAsync(string recipientIdentifier, OtpType? type = null, int limit = 50)
    {
        var query = _context.Otps
            .Where(o => o.RecipientIdentifier == recipientIdentifier);
        
        if (type.HasValue)
        {
            query = query.Where(o => o.Type == type.Value);
        }
        
        return await query
            .OrderByDescending(o => o.GeneratedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetExpiredOtpIdsAsync(DateTime olderThan)
    {
        return await _context.Otps
            .Where(o => o.CreatedAt < olderThan)
            .Select(o => o.Id)
            .ToListAsync();
    }

    public async Task DeleteByIdsAsync(List<Guid> ids)
    {
        if (!ids.Any()) return;
        
        await _context.Otps
            .Where(o => ids.Contains(o.Id))
            .ExecuteDeleteAsync();
        
        _logger.LogInformation("Deleted {Count} expired OTPs", ids.Count);
    }

    public async Task<Dictionary<string, object>> GetAnalyticsAsync(DateTime startDate, DateTime endDate, Guid? companyId = null)
    {
        var query = _context.Otps
            .Where(o => o.GeneratedAt >= startDate && o.GeneratedAt <= endDate);
        
        if (companyId.HasValue)
        {
            query = query.Where(o => o.CompanyId == companyId.Value);
        }
        
        var otps = await query.ToListAsync();
        
        var analytics = new Dictionary<string, object>
        {
            ["TotalGenerated"] = otps.Count,
            ["TotalValidated"] = otps.Count(o => o.IsUsed),
            ["TotalFailed"] = otps.Count(o => o.AttemptCount > 0 && !o.IsUsed),
            ["TotalExpired"] = otps.Count(o => o.ExpiresAt < DateTime.UtcNow && !o.IsUsed),
            ["TotalLocked"] = otps.Count(o => o.IsLocked),
            ["SuccessRate"] = otps.Any() ? (decimal)otps.Count(o => o.IsUsed) / otps.Count * 100 : 0,
            ["GenerationsByType"] = otps.GroupBy(o => o.Type).ToDictionary(g => g.Key.ToString(), g => g.Count()),
            ["DeliveriesByChannel"] = otps.GroupBy(o => o.DeliveryChannel).ToDictionary(g => g.Key.ToString(), g => g.Count())
        };
        
        // Calculate average validation time for successful OTPs
        var successfulOtps = otps.Where(o => o.IsUsed && o.UsedAt.HasValue).ToList();
        if (successfulOtps.Any())
        {
            var avgSeconds = successfulOtps
                .Average(o => (o.UsedAt!.Value - o.GeneratedAt).TotalSeconds);
            analytics["AverageValidationTimeSeconds"] = avgSeconds;
        }
        else
        {
            analytics["AverageValidationTimeSeconds"] = 0;
        }
        
        return analytics;
    }

    public async Task<List<Otp>> GetByCompanyIdAsync(Guid companyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Otps
            .Where(o => o.CompanyId == companyId);
        
        if (startDate.HasValue)
        {
            query = query.Where(o => o.GeneratedAt >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(o => o.GeneratedAt <= endDate.Value);
        }
        
        return await query
            .OrderByDescending(o => o.GeneratedAt)
            .ToListAsync();
    }
}
