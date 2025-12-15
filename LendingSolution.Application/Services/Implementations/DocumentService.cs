using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Implementations;

public class DocumentService(
    IDocumentRepository documentRepository,
    IThirdPartyDocumentService thirdPartyDocumentService) : IDocumentService
{
    public async Task<DocumentUploadResultDto> UploadDocumentAsync(UploadDocumentDto uploadDto, string userId)
    {
        // Upload to third-party service
        var uploadResult = await thirdPartyDocumentService.UploadDocumentAsync(
            uploadDto.Base64String,
            uploadDto.DocumentName,
            uploadDto.FileExtension ?? string.Empty);

        if (!uploadResult.Success)
        {
            throw new AppException($"Failed to upload document: {uploadResult.ErrorMessage}", 500);
        }

        // Create document record in database
        var document = new Document
        {
            DocumentName = uploadDto.DocumentName,
            DocumentType = GetFileTypeFromExtension(uploadDto.FileExtension ?? string.Empty),
            DocumentUrl = uploadResult.Url,
        };

        var savedDocument = await documentRepository.CreateDocumentAsync(document);

        return new DocumentUploadResultDto
        {
            Id = savedDocument.Id, // Database ID for fetching
            DocumentId = uploadResult.DocumentId, // Firebase document ID
            Url = uploadResult.Url,
            DocumentName = uploadDto.DocumentName,
            UploadedAt = savedDocument.UploadedAt ?? DateTime.UtcNow
        };
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(string id)
    {
        var document = await documentRepository.GetDocumentByIdAsync(id);
        if (document == null)
        {
            return null;
        }

        return MapToDto(document);
    }

    public async Task<List<DocumentDto>> GetAllDocumentsAsync()
    {
        var documents = await documentRepository.GetAllDocumentsAsync();
        return documents.Select(MapToDto).ToList();
    }

    public async Task<List<DocumentDto>> GetDocumentsByTypeAsync(string documentType)
    {
        var documents = await documentRepository.GetDocumentsByTypeAsync(documentType);
        return documents.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteDocumentAsync(string id)
    {
        var document = await documentRepository.GetDocumentByIdAsync(id);
        if (document == null)
        {
            throw new AppException("Document not found", 404);
        }

        var result = await documentRepository.DeleteDocumentAsync(id);
        return result;
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType,
            DocumentUrl = document.DocumentUrl,
            Status = document.Status,
            ErrorMessage = document.ErrorMessage,
            FileExtension = document.FileExtension,
            CreatedAt = document.CreatedAt,
            UploadedAt = document.UploadedAt
        };
    }

    private static string GetFileTypeFromExtension(string extension)
    {
        var ext = extension.ToLowerInvariant().TrimStart('.');

        return ext switch
        {
            "pdf" => "PDF",
            "doc" or "docx" => "Word Document",
            "xls" or "xlsx" => "Excel Spreadsheet",
            "ppt" or "pptx" => "PowerPoint Presentation",
            "jpg" or "jpeg" => "JPEG Image",
            "png" => "PNG Image",
            "gif" => "GIF Image",
            "txt" => "Text File",
            "zip" => "Archive",
            "rar" => "Archive",
            _ => "Unknown"
        };
    }
}
