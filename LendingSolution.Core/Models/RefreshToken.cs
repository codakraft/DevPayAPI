using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class RefreshToken
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Token { get; set; } = string.Empty;
    
    [Required]
    public DateTime ExpiryDate { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsRevoked { get; set; } = false;
    
    public DateTime? RevokedAt { get; set; }
    
    [MaxLength(450)]
    public string? RevokedByUserId { get; set; }
    
    [MaxLength(500)]
    public string? ReasonRevoked { get; set; }
    
    [MaxLength(500)]
    public string? ReplacedByToken { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationUser? RevokedByUser { get; set; }
    
    // Helper properties
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
}
