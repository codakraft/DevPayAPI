using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Minimal BVN tracking - only verification status for data protection compliance
/// Auto-deletes after 90 days to comply with data retention policies
/// </summary>
public class MonoBvnVerificationRecord
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(64)]
    public string BvnHash { get; set; } = string.Empty; // SHA-256 hash of BVN for privacy
    
    public bool IsVerified { get; set; }
    public DateTime VerifiedAt { get; set; }
    public DateTime ExpiresAt { get; set; } // Auto-delete after 90 days
    
    [Required]
    [MaxLength(10)]
    public string Provider { get; set; } = string.Empty; // 'xds' or 'cdc'
    
    // Minimal required data only (from BVN verification)
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string DateOfBirth { get; set; } = string.Empty; // YYYY-MM-DD format
    
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Session tracking for multi-step verification
    [MaxLength(100)]
    public string? LastSessionId { get; set; }
    
    // Audit trail
    [Required]
    [MaxLength(50)]
    public string VerifiedByUserId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}