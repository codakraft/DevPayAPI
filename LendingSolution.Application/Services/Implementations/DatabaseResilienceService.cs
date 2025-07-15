using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Data;

namespace LendingSolution.Application.Services.Implementations;

public interface IDatabaseResilienceService
{
    Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default);
    Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}

public class DatabaseResilienceService : IDatabaseResilienceService
{
    private readonly ILogger<DatabaseResilienceService> _logger;
    private readonly IAsyncPolicy _retryPolicy;

    public DatabaseResilienceService(ILogger<DatabaseResilienceService> logger)
    {
        _logger = logger;
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await operation();
        });
    }

    public async Task ExecuteWithRetryAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            await operation();
        });
    }

    private IAsyncPolicy CreateRetryPolicy()
    {
        return Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .Or<TimeoutException>()
            .Or<InvalidOperationException>(ex => ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var exception = outcome as Exception;
                    _logger.LogWarning(
                        "Database operation failed. Retry {RetryCount} will execute in {Delay}ms. Exception: {Exception}",
                        retryCount,
                        timespan.TotalMilliseconds,
                        exception?.Message ?? "Unknown error");
                }
            );
    }

    private static bool IsTransientError(SqlException ex)
    {
        // Check for transient SQL errors that should trigger retry
        var transientErrorNumbers = new[]
        {
            4060,  // Database does not exist
            40197, // Service has encountered an error processing your request
            40501, // Service is currently busy
            40613, // Database not available
            49918, // Cannot process request. Not enough resources
            49919, // Cannot process create or update request
            49920, // Cannot process request. Too many operations in progress
            4221,  // Login failed for user (read-only routing)
            18456, // Login failed for user (general)
            2,     // Timeout expired
            53,    // Network path not found
            233,   // Connection was not properly initialized
            10053, // Existing connection was forcibly closed
            10054, // Connection was reset by peer
            11001, // No such host is known
            -2,    // Timeout expired (SqlException.Number = -2)
            20,    // Instance failure
            64,    // Host not reachable
            1205,  // Deadlock victim
            1222   // Lock request timeout
        };

        return transientErrorNumbers.Contains(ex.Number);
    }
}
