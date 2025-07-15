# Database Connection Retry Implementation

## Overview
This implementation provides comprehensive database connection resilience for the LendingSolution platform, specifically optimized for Azure SQL Database. It includes automatic retry policies, health checks, and manual resilience services for complex database operations.

## Current Database Setup Analysis
- **Database**: Azure SQL Database (`lending-app.database.windows.net`)
- **Provider**: SQL Server with Entity Framework Core
- **Connection**: Azure SQL with encrypted connection and 30-second timeout
- **Previous State**: Basic connection without retry policies

## Implemented Features

### 1. Automatic EF Core Retry Policy
**Location**: `ServiceExtensions.cs > ConfigureSqlContext()`

**Configuration**:
- **Max Retry Count**: 5 (Production), 3 (Development)
- **Max Retry Delay**: 30 seconds (Production), 15 seconds (Development)
- **Command Timeout**: 60 seconds (Production), 45 seconds (Development)
- **Configurable**: Settings can be modified via `appsettings.json`

**Transient Errors Handled**:
```csharp
// Azure SQL Database specific errors
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
```

### 2. Configuration Management
**Model**: `DatabaseRetryPolicySettings.cs`

**Settings Structure**:
```json
{
  "DatabaseRetryPolicy": {
    "MaxRetryCount": 5,
    "MaxRetryDelaySeconds": 30,
    "CommandTimeoutSeconds": 60,
    "EnableRetryOnFailure": true
  }
}
```

**Environment-Specific Settings**:
- **Production** (`appsettings.json`): More aggressive retry (5 retries, 30s delay)
- **Development** (`appsettings.Development.json`): Faster feedback (3 retries, 15s delay)

### 3. Database Health Checks
**Service**: `DatabaseHealthCheckService.cs`

**Endpoints**:
- `GET /health` - Overall health status
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

**Features**:
- Simple connectivity test (`SELECT 1`)
- Logging of health check results
- Integration with ASP.NET Core Health Checks

### 4. Manual Resilience Service
**Service**: `IDatabaseResilienceService` / `DatabaseResilienceService.cs`

**Purpose**: For complex operations requiring manual retry logic beyond EF Core's automatic retry

**Features**:
- **Polly-based retry policies** with exponential backoff
- **Configurable retry count** and delay
- **Comprehensive error handling** for SQL transient errors
- **Detailed logging** of retry attempts

**Usage Example**:
```csharp
public class SomeService
{
    private readonly IDatabaseResilienceService _resilienceService;
    
    public async Task<Result> ComplexOperationAsync()
    {
        return await _resilienceService.ExecuteWithRetryAsync(async () =>
        {
            // Complex database operation that needs manual retry handling
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Multiple operations
                await operation1();
                await operation2();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
```

### 5. Example Implementation
**Service**: `ResilientDatabaseService.cs`

Demonstrates practical usage patterns for:
- Complex database operations with transactions
- Database statistics gathering
- Proper error handling and logging

## Configuration Options

### AppSettings Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:lending-app.database.windows.net,1433;Initial Catalog=LendingDev;Persist Security Info=False;User ID=adminUser;Password=Pass@LendingUser;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "DatabaseRetryPolicy": {
    "MaxRetryCount": 5,
    "MaxRetryDelaySeconds": 30,
    "CommandTimeoutSeconds": 60,
    "EnableRetryOnFailure": true
  }
}
```

### Registration in Program.cs
```csharp
// Configure database with retry policies
builder.Services.ConfigureSqlContext(builder.Configuration);

// Configure health checks
builder.Services.ConfigureHealthChecks(builder.Configuration);

// Configure database resilience service
builder.Services.ConfigureDatabaseResilience();

// Map health check endpoints
app.MapHealthChecks("/health");
```

## Benefits

### 1. **Automatic Resilience**
- EF Core operations automatically retry on transient failures
- No code changes needed for existing database operations
- Comprehensive coverage of Azure SQL Database error scenarios

### 2. **Configurable Behavior**
- Environment-specific retry settings
- Easy adjustment without code changes
- Ability to disable retry for testing scenarios

### 3. **Manual Control**
- Additional resilience service for complex scenarios
- Polly-based policies with exponential backoff
- Detailed logging and monitoring

### 4. **Health Monitoring**
- Real-time connectivity monitoring
- Integration with monitoring tools
- Proactive issue detection

### 5. **Production Ready**
- Optimized for Azure SQL Database
- Comprehensive error handling
- Performance considerations (command timeouts)

## Best Practices Implemented

### 1. **Retry Strategy**
- **Exponential backoff** to avoid overwhelming the database
- **Maximum retry limits** to prevent infinite loops
- **Specific error targeting** to avoid retrying non-transient errors

### 2. **Logging**
- **Structured logging** with retry attempt details
- **Error categorization** for monitoring
- **Performance metrics** tracking

### 3. **Configuration**
- **Environment-specific** settings
- **Runtime configurability** via appsettings
- **Sensible defaults** with override capability

### 4. **Separation of Concerns**
- **Automatic retry** for standard EF operations
- **Manual retry** for complex business operations
- **Health checks** for monitoring

## Usage Guidelines

### When to Use Automatic Retry (EF Core)
- Standard CRUD operations
- Simple queries and updates
- Most business operations

### When to Use Manual Retry (Resilience Service)
- Complex transactions with multiple operations
- Operations requiring custom retry logic
- Long-running database operations
- Operations with specific business rules for retry

### Health Check Endpoints
- **Development**: Monitor during testing
- **Production**: Integrate with load balancers and monitoring tools
- **DevOps**: Use for automated health verification

## Monitoring and Troubleshooting

### Logs to Monitor
```
- "Database health check passed/failed"
- "Database operation failed. Retry {RetryCount} will execute in {Delay}ms"
- EF Core retry attempt logs (if enabled)
```

### Key Metrics
- Health check success rate
- Retry attempt frequency
- Database operation latency
- Connection timeout occurrences

### Troubleshooting
1. **High retry rates**: Check Azure SQL DTU/resource utilization
2. **Health check failures**: Verify connection string and firewall rules
3. **Timeout issues**: Consider increasing CommandTimeout or optimizing queries
4. **Deadlocks**: Review database design and transaction scope

## Dependencies Added
```xml
<PackageReference Include="Polly" Version="8.2.0" />
<PackageReference Include="Polly.Extensions.Http" Version="3.0.0" />
```

## Files Modified/Created

### New Files
- `DatabaseRetryPolicySettings.cs` - Configuration model
- `DatabaseHealthCheckService.cs` - Health check implementation
- `DatabaseResilienceService.cs` - Manual retry service
- `ResilientDatabaseService.cs` - Usage examples

### Modified Files
- `ServiceExtensions.cs` - Added retry configuration and service registration
- `Program.cs` - Added health checks and resilience service configuration
- `appsettings.json` / `appsettings.Development.json` - Added retry policy settings
- `LendingSolution.Application.csproj` - Added Polly dependencies

This implementation provides a robust, production-ready solution for database connectivity issues commonly encountered with cloud databases like Azure SQL Database.
