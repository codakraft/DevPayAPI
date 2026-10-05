namespace LendingSolution.Core.Models;

/// <summary>
/// Audit log for tracking critical system operations
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // What happened
    public string Action { get; set; } = string.Empty;  // "LoanApproved", "WalletDebited"
    public string Category { get; set; } = string.Empty;  // "Financial", "Loan", "User", "System"
    
    // Who did it
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }  // Full name of the user who acted, filled from UserId
    
    // What was affected
    public string? EntityType { get; set; }  // "Loan", "Wallet", "User"
    public string? EntityId { get; set; }
    
    // Multi-tenant
    public Guid? CompanyId { get; set; }
    
    // Details (flexible JSON)
    public string? Details { get; set; }  // Store whatever you want here
    
    // Financial operations
    public decimal? Amount { get; set; }
    public decimal? OldBalance { get; set; }
    public decimal? NewBalance { get; set; }
    
    // Context
    public string? IpAddress { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
