using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class MfaSessionRepository(ApplicationDbContext context) : IMfaSessionRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<MfaSession?> GetBySessionIdAsync(string sessionId)
    {
        return await _context.MfaSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);
    }

    public async Task CreateAsync(MfaSession session)
    {
        _context.MfaSessions.Add(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string sessionId)
    {
        var session = await _context.MfaSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (session != null)
        {
            _context.MfaSessions.Remove(session);
            await _context.SaveChangesAsync();
        }
    }

    public async Task CleanupExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var expired = await _context.MfaSessions
            .Where(s => s.ExpiresAt < now)
            .ToListAsync();

        if (expired.Count > 0)
        {
            _context.MfaSessions.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }
}
