using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByIdAsync(string id);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<IEnumerable<RefreshToken>> GetAllTokensByUserIdAsync(string userId);
    Task<RefreshToken> CreateTokenAsync(RefreshToken refreshToken);
    Task UpdateTokenAsync(RefreshToken refreshToken);
    Task DeleteTokenAsync(string id);
    Task DeleteExpiredTokensAsync();
    Task RevokeTokenAsync(string token, string? revokedBy = null, string? reason = null);
    Task RevokeAllUserTokensAsync(string userId, string? revokedBy = null, string? reason = null);
    Task<bool> IsTokenActiveAsync(string token);
}
