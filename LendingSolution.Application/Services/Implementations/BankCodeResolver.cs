using LendingSolution.Application.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <inheritdoc />
/// <remarks>
/// Backed by Mono's bank list, which returns both code systems on every entry
/// (bank_code = CBN, nip_code = NIBSS), so the mapping is authoritative rather than
/// inferred from bank names. Name matching would be unsafe here: the list contains
/// near-duplicates such as "STERLING BANK" and "STERLING MOBILE" that differ by one
/// word but are separate institutions.
///
/// Registered as a singleton so the list is fetched once per day rather than per
/// disbursement; IMonoService is scoped, so it is resolved through a scope factory.
/// </remarks>
public class BankCodeResolver : IBankCodeResolver
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BankCodeResolver> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private Dictionary<string, string>? _cbnToNip;
    private DateTime _lastRefreshedAt = DateTime.MinValue;

    public BankCodeResolver(IServiceScopeFactory scopeFactory, ILogger<BankCodeResolver> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string?> ResolveNipCodeAsync(string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
        {
            return null;
        }

        var normalized = bankCode.Trim();

        var map = await GetMapAsync();
        if (map == null)
        {
            return null;
        }

        if (map.TryGetValue(normalized, out var nipCode))
        {
            return nipCode;
        }

        // Already a NIBSS code. CBN codes in circulation are 3 or 9 digits, so a value
        // that appears as a nip_code in the list is unambiguously already translated.
        if (map.Values.Contains(normalized))
        {
            _logger.LogInformation("Bank code {BankCode} is already in NIP format", normalized);
            return normalized;
        }

        _logger.LogError(
            "Unable to resolve bank code {BankCode} to a NIP code. {KnownCount} banks known.",
            normalized, map.Count);

        return null;
    }

    private async Task<Dictionary<string, string>?> GetMapAsync()
    {
        if (_cbnToNip != null && DateTime.UtcNow - _lastRefreshedAt < CacheLifetime)
        {
            return _cbnToNip;
        }

        await _refreshLock.WaitAsync();
        try
        {
            // Another caller may have refreshed while this one waited.
            if (_cbnToNip != null && DateTime.UtcNow - _lastRefreshedAt < CacheLifetime)
            {
                return _cbnToNip;
            }

            var refreshed = await FetchMapAsync();
            if (refreshed != null)
            {
                _cbnToNip = refreshed;
                _lastRefreshedAt = DateTime.UtcNow;
                return _cbnToNip;
            }

            // Serving a stale list beats failing a disbursement: bank codes change rarely,
            // and a Mono outage should not stop money moving.
            if (_cbnToNip != null)
            {
                _logger.LogWarning(
                    "Bank list refresh failed; serving cached list from {LastRefreshed:u}",
                    _lastRefreshedAt);
            }

            return _cbnToNip;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<Dictionary<string, string>?> FetchMapAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var monoService = scope.ServiceProvider.GetRequiredService<IMonoService>();

        var response = await monoService.GetBanksAsync();
        if (response?.Data == null || response.Data.Count == 0)
        {
            _logger.LogError("Mono bank list returned no entries; cannot build bank code map");
            return null;
        }

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var bank in response.Data)
        {
            if (string.IsNullOrWhiteSpace(bank.BankCode) || string.IsNullOrWhiteSpace(bank.NipCode))
            {
                continue;
            }

            map[bank.BankCode.Trim()] = bank.NipCode.Trim();
        }

        if (map.Count == 0)
        {
            _logger.LogError("Mono returned {Count} banks but none carried both codes", response.Data.Count);
            return null;
        }

        _logger.LogInformation("Refreshed bank code map with {Count} banks from Mono", map.Count);
        return map;
    }
}
