namespace LendingSolution.Core.Settings;

public class DatabaseRetryPolicySettings
{
    public int MaxRetryCount { get; set; } = 5;
    public int MaxRetryDelaySeconds { get; set; } = 30;
    public int CommandTimeoutSeconds { get; set; } = 60;
    public bool EnableRetryOnFailure { get; set; } = true;
    
    // Common Azure SQL Database transient error codes
    public static readonly int[] AzureSqlTransientErrors = new int[]
    {
        4060,  // Database does not exist
        40197, // Service has encountered an error processing your request
        40501, // Service is currently busy
        40613, // Database not available
        49918, // Cannot process request. Not enough resources to process request
        49919, // Cannot process create or update request. Too many create or update operations in progress
        49920, // Cannot process request. Too many operations in progress
        4221,  // Login failed for user (read-only routing)
        18456, // Login failed for user (general)
        2,     // Timeout expired
        53,    // Network path not found
        233,   // Connection was not properly initialized
        10053, // Existing connection was forcibly closed by remote host
        10054, // Connection was reset by peer
        11001, // No such host is known
        -2,    // Timeout expired (SqlException.Number = -2)
        20,    // Instance failure
        64,    // Host not reachable
        1205,  // Deadlock victim
        1222   // Lock request timeout
    };
}
