using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Stores Remita customer authorization codes for reuse
/// </summary>
public class RemitaCustomer : Base
{
    [MaxLength(100)]
    public string CustomerId { get; set; } = string.Empty;

    [MaxLength(255)]
    [Required]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string AuthorisationCode { get; set; } = string.Empty;

    public DateTime? LastUsedAt { get; set; }
}
