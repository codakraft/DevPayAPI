using LendingSolution.Application.Services.Implementations;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Example service demonstrating how to use the database resilience service
/// for operations that need manual retry logic beyond EF Core's built-in retry
/// </summary>
public class ResilientDatabaseService
{
    private readonly ApplicationDbContext _context;
    private readonly IDatabaseResilienceService _resilienceService;
    private readonly ILogger<ResilientDatabaseService> _logger;

    public ResilientDatabaseService(
        ApplicationDbContext context,
        IDatabaseResilienceService resilienceService,
        ILogger<ResilientDatabaseService> logger)
    {
        _context = context;
        _resilienceService = resilienceService;
        _logger = logger;
    }

    /// <summary>
    /// Example: Execute a complex database operation with manual retry
    /// Use this pattern for operations that require additional resilience beyond EF Core's built-in retry
    /// </summary>
    public async Task<bool> ExecuteComplexDatabaseOperationAsync()
    {
        return await _resilienceService.ExecuteWithRetryAsync(async () =>
        {
            _logger.LogInformation("Executing complex database operation");
            
            // Example complex operation that might need manual retry handling
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Multiple database operations
                var companyCount = await _context.Companies.CountAsync();
                var loanCount = await _context.Loans.CountAsync();
                
                // Some business logic
                _logger.LogInformation("Companies: {CompanyCount}, Loans: {LoanCount}", companyCount, loanCount);
                
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    /// <summary>
    /// Example: Get database statistics with retry
    /// </summary>
    public async Task<Dictionary<string, int>> GetDatabaseStatisticsAsync()
    {
        return await _resilienceService.ExecuteWithRetryAsync(async () =>
        {
            var stats = new Dictionary<string, int>
            {
                ["Companies"] = await _context.Companies.CountAsync(),
                ["Loans"] = await _context.Loans.CountAsync(),
                ["Users"] = await _context.Users.CountAsync(),
                ["LoanProducts"] = await _context.LoanProducts.CountAsync()
            };

            _logger.LogInformation("Database statistics retrieved successfully");
            return stats;
        });
    }
}
