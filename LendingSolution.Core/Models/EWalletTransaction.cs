using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class EWalletTransaction
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string TransactionReference { get; set; } = string.Empty;

    [MaxLength(20)]
    public string FromAccount { get; set; } = string.Empty;

    [MaxLength(20)]
    public string ToAccount { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? RawResponse { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
