using System.ComponentModel.DataAnnotations;
using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Dtos;

public class DocumentDto
{
    public string Id { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public string? DocumentType { get; set; }
    public string? DocumentUrl { get; set; }
    public DocumentStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FileExtension { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UploadedAt { get; set; }
}

public class MultipartUploadResponseDto
{
    public List<DocumentUploadPendingDto> Documents { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

public class DocumentUploadPendingDto
{
    public string Id { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UploadDocumentDto
{
    [Required(ErrorMessage = "Document name is required")]
    [MaxLength(255, ErrorMessage = "Document name cannot exceed 255 characters")]
    public string DocumentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Base64 string is required")]
    public string Base64String { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "File extension cannot exceed 50 characters")]
    public string? FileExtension { get; set; }
}

public class DocumentUploadResultDto
{
    public string DocumentId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class ThirdPartyUploadResultDto
{
    public bool Success { get; set; }
    public string DocumentId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ThirdPartyDocumentDetailsDto
{
    public string DocumentId { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
