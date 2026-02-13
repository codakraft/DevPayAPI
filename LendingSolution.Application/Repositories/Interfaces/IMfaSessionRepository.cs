using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IMfaSessionRepository
{
    Task<MfaSession?> GetBySessionIdAsync(string sessionId);
    Task CreateAsync(MfaSession session);
    Task DeleteAsync(string sessionId);
    Task CleanupExpiredAsync();
}
