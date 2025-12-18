using System.ComponentModel.DataAnnotations;
using LendingSolution.Core.Enum;

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

    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    [MaxLength(255)]
    public string? UploadedBy { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Completed;

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [MaxLength(50)]
    public string? FileExtension { get; set; }

    public long? FileSizeBytes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UploadedAt { get; set; }
}
