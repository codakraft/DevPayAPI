using System.Security.Claims;
using LendingSolution.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace LendingSolution.API.Auth;

/// <summary>
/// Rejects access tokens whose user has been deactivated or whose security stamp has
/// changed since the token was issued (role change, password change, deactivation).
/// Matching lookups are cached briefly, so a revocation takes effect within <see cref="CacheDuration"/>;
/// a token newer than the cache is confirmed against the database immediately.
/// </summary>
public class UserSessionValidator(UserManager<ApplicationUser> userManager, IMemoryCache cache)
{
    public const string SecurityStampClaim = "sstamp";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    private sealed record SessionState(string? SecurityStamp, bool IsActive);

    public async Task<bool> IsValidAsync(ClaimsPrincipal principal)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var tokenStamp = principal.FindFirstValue(SecurityStampClaim);

        // Tokens issued before this check existed carry no stamp; force a fresh login
        if (string.IsNullOrEmpty(userId) || tokenStamp is null)
        {
            return false;
        }

        var cacheKey = $"session:{userId}";
        if (cache.TryGetValue(cacheKey, out SessionState? cached) && Matches(cached, tokenStamp))
        {
            return true;
        }

        // Cache miss, or a mismatch that may only mean the cache is stale (e.g. a token issued
        // right after a role or password change): check the database once before rejecting.
        var user = await userManager.FindByIdAsync(userId);
        var state = user is null ? null : new SessionState(user.SecurityStamp, user.IsActive);
        cache.Set(cacheKey, state, CacheDuration);

        return Matches(state, tokenStamp);
    }

    private static bool Matches(SessionState? state, string tokenStamp) =>
        state is not null && state.IsActive && (state.SecurityStamp ?? string.Empty) == tokenStamp;
}
