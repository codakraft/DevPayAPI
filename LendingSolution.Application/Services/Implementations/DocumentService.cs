using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IThirdPartyDocumentService _thirdPartyDocumentService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        IDocumentRepository documentRepository,
        IThirdPartyDocumentService thirdPartyDocumentService,
        ILogger<DocumentService> logger)
    {
        _documentRepository = documentRepository;
        _thirdPartyDocumentService = thirdPartyDocumentService;
        _logger = logger;
    }

    public async Task<DocumentUploadResultDto> UploadDocumentAsync(UploadDocumentDto uploadDto, string userId)
    {
        try
        {
            _logger.LogInformation("Starting document upload for user {UserId}, document: {DocumentName}", userId, uploadDto.DocumentName);

            // Upload to third-party service
            var uploadResult = await _thirdPartyDocumentService.UploadDocumentAsync(
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

            var savedDocument = await _documentRepository.CreateDocumentAsync(document);

            _logger.LogInformation("Document uploaded successfully. DocumentId: {DocumentId}, DatabaseId: {DatabaseId}",
                uploadResult.DocumentId, savedDocument.Id);

            return new DocumentUploadResultDto
            {
                DocumentId = uploadResult.DocumentId,
                Url = uploadResult.Url,
                DocumentName = uploadDto.DocumentName
            };
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document upload for user {UserId}", userId);
            throw new AppException("An unexpected error occurred during document upload", 500);
        }
    }

    public async Task<DocumentDto?> GetDocumentByIdAsync(string id)
    {
        try
        {
            var document = await _documentRepository.GetDocumentByIdAsync(id);
            if (document == null)
            {
                return null;
            }

            return MapToDto(document);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document by ID: {DocumentId}", id);
            throw new AppException("Error retrieving document", 500);
        }
    }

    public async Task<List<DocumentDto>> GetAllDocumentsAsync()
    {
        try
        {
            var documents = await _documentRepository.GetAllDocumentsAsync();
            return documents.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all documents");
            throw new AppException("Error retrieving documents", 500);
        }
    }

    public async Task<List<DocumentDto>> GetDocumentsByTypeAsync(string documentType)
    {
        try
        {
            var documents = await _documentRepository.GetDocumentsByTypeAsync(documentType);
            return documents.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documents for type: {DocumentType}", documentType);
            throw new AppException("Error retrieving documents by type", 500);
        }
    }

    public async Task<bool> DeleteDocumentAsync(string id)
    {
        try
        {
            var document = await _documentRepository.GetDocumentByIdAsync(id);
            if (document == null)
            {
                throw new AppException("Document not found", 404);
            }

            var result = await _documentRepository.DeleteDocumentAsync(id);

            _logger.LogInformation("Document deleted successfully. DocumentId: {DocumentId}", id);
            return result;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during document deletion. DocumentId: {DocumentId}", id);
            throw new AppException("An unexpected error occurred during document deletion", 500);
        }
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            DocumentName = document.DocumentName,
            DocumentType = document.DocumentType,
            DocumentUrl = document.DocumentUrl,
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
