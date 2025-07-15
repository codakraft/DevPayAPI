using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

public class DatabaseHealthCheckService : IHealthCheck
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseHealthCheckService> _logger;

    public DatabaseHealthCheckService(ApplicationDbContext context, ILogger<DatabaseHealthCheckService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to perform a simple database operation
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            
            _logger.LogInformation("Database health check passed");
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database connection failed", ex);
        }
    }
}
