using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Infrastructure.Data;

namespace LendingSolution.Application.Repositories.Implementations;

public class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetByIdAsync(string id)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Id == id);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<RefreshToken>> GetAllTokensByUserIdAsync(string userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .OrderByDescending(rt => rt.CreatedAt)
            .ToListAsync();
    }

    public async Task<RefreshToken> CreateTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task UpdateTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTokenAsync(string id)
    {
        var token = await GetByIdAsync(id);
        if (token != null)
        {
            _context.RefreshTokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate <= DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeTokenAsync(string token, string? revokedBy = null, string? reason = null)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedByUserId = revokedBy;
            refreshToken.ReasonRevoked = reason;
            
            await UpdateTokenAsync(refreshToken);
        }
    }

    public async Task RevokeAllUserTokensAsync(string userId, string? revokedBy = null, string? reason = null)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId);
        
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByUserId = revokedBy;
            token.ReasonRevoked = reason ?? "All tokens revoked";
        }

        if (activeTokens.Any())
        {
            _context.RefreshTokens.UpdateRange(activeTokens);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsTokenActiveAsync(string token)
    {
        var refreshToken = await GetByTokenAsync(token);
        return refreshToken?.IsActive ?? false;
    }
}
