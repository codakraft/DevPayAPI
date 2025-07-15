using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class Document
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DocumentType { get; set; }

    [Required]
    [MaxLength(500)]
    public string DocumentUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
