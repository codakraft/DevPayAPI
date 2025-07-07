using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class AdminSettings
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;
    
    [Required]
    public string SettingValue { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(450)]
    public string? CreatedBy { get; set; }
    
    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}

public class Approval
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string ApprovalType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(450)]
    public string ReferenceId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(450)]
    public string RequestedBy { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(1000)]
    public string? Reason { get; set; }
    
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    [MaxLength(450)]
    public string? ProcessedBy { get; set; }
    
    public Guid? CompanyId { get; set; } // For multi-tenant support
    
    // Navigation properties
    public virtual ApplicationUser? RequestedByUser { get; set; }
    public virtual ApplicationUser? ProcessedByUser { get; set; }
    public virtual Company? Company { get; set; }
}
