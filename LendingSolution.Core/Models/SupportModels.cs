using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingSolution.Core.Models;

public class SupportTicket
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [ForeignKey(nameof(Company))]
    public Guid CompanyId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Account, Loan, Payment, Technical, Other
    
    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Open"; // Open, InProgress, Resolved, Closed
    
    [MaxLength(450)] // ASP.NET Identity user ID length
    public string? AssignedTo { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
    public virtual ApplicationUser? AssignedToUser { get; set; }
    public virtual ICollection<SupportComment> Comments { get; set; } = new List<SupportComment>();
}

public class SupportComment
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [ForeignKey(nameof(Ticket))]
    public string TicketId { get; set; } = string.Empty;
    
    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
    
    public bool IsInternal { get; set; } = false;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual SupportTicket Ticket { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}

public enum SupportTicketCategory
{
    Account,
    Loan,
    Payment,
    Technical,
    Other
}

public enum SupportTicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum SupportTicketStatus
{
    Open,
    InProgress,
    Resolved,
    Closed
}