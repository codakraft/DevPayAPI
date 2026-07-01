using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LendingSolution.Infrastructure.Logging;

/// <summary>
/// Logger provider that writes log entries to the AppLogs database table.
/// Uses batched writes with a background timer to minimize performance impact.
/// </summary>
public sealed class DatabaseLoggerProvider : ILoggerProvider
{
    private readonly string _connectionString;
    internal readonly LogLevel _MinLevel;
    private readonly ConcurrentQueue<LogEntry> _logQueue = new();
    private readonly Timer _flushTimer;
    private readonly SemaphoreSlim _flushLock = new(1, 1);
    private bool _disposed;

    public DatabaseLoggerProvider(string connectionString, LogLevel minLevel = LogLevel.Information)
    {
        _connectionString = connectionString;
        _MinLevel = minLevel;

        // Flush every 5 seconds
        _flushTimer = new Timer(_ => _ = FlushAsync(), null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new DatabaseLogger(categoryName, this);
    }

    internal void AddLogEntry(LogEntry entry)
    {
        _logQueue.Enqueue(entry);

        // If queue is getting large, trigger an immediate flush
        if (_logQueue.Count >= 50)
        {
            _ = FlushAsync();
        }
    }

    private async Task FlushAsync()
    {
        if (_disposed || _logQueue.IsEmpty) return;
        if (!_flushLock.Wait(0)) return; // Skip if another flush is already running

        try
        {
            var entries = new List<LogEntry>();
            while (entries.Count < 100 && _logQueue.TryDequeue(out var entry))
            {
                entries.Add(entry);
            }

            if (entries.Count == 0) return;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            foreach (var entry in entries)
            {
                try
                {
                    const string sql = @"
                        INSERT INTO AppLogs (Timestamp, Level, Category, Message, Exception, EventId)
                        VALUES (@Timestamp, @Level, @Category, @Message, @Exception, @EventId)";

                    using var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@Timestamp", entry.Timestamp);
                    command.Parameters.AddWithValue("@Level", entry.Level);
                    command.Parameters.AddWithValue("@Category", (object?)Truncate(entry.Category, 512) ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Message", (object?)entry.Message ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Exception", (object?)entry.Exception ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EventId", (object?)Truncate(entry.EventId, 100) ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
                catch
                {
                    // Swallow individual insert errors to avoid losing the entire batch
                }
            }
        }
        catch
        {
            // Swallow flush errors — logging should never crash the app
        }
        finally
        {
            _flushLock.Release();
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (value == null) return null;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _flushTimer.Dispose();

        // Final flush on shutdown
        FlushAsync().GetAwaiter().GetResult();
        _flushLock.Dispose();
    }
}

/// <summary>
/// Individual logger instance that queues log entries for batched database writes
/// </summary>
internal sealed class DatabaseLogger(string categoryName, DatabaseLoggerProvider provider) : ILogger
{
    // Skip noisy framework categories to keep the logs table useful
    private static readonly HashSet<string> ExcludedPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Hosting",
        "Microsoft.Extensions",
        "System.Net.Http"
    };

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None) return false;

        // Check excluded prefixes
        foreach (var prefix in ExcludedPrefixes)
        {
            if (categoryName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return logLevel >= provider._MinLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);

        provider.AddLogEntry(new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = logLevel.ToString(),
            Category = categoryName,
            Message = message,
            Exception = exception?.ToString(),
            EventId = eventId.Id != 0 ? eventId.ToString() : null
        });
    }
}

/// <summary>
/// Internal log entry queued for batched database write
/// </summary>
internal class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
    public string? EventId { get; set; }
}
