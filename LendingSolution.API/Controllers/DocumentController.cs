using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Application.Exceptions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using LendingSolution.Application.Repositories.Interfaces;

using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
// [Authorize]
[AllowAnonymous]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IThirdPartyDocumentService _firebaseService;
    private readonly ILogger<DocumentController> _logger;
    private readonly IWebHostEnvironment _environment;

    public DocumentController(
        IDocumentService documentService,
        IDocumentRepository documentRepository,
        IThirdPartyDocumentService firebaseService,
        ILogger<DocumentController> logger,
        IWebHostEnvironment environment)
    {
        _documentService = documentService;
        _documentRepository = documentRepository;
        _firebaseService = firebaseService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Test Firebase connectivity
    /// </summary>
    [HttpGet("test-firebase")]
    public async Task<IActionResult> TestFirebase()
    {
        try
        {
            // Test with a small dummy file
            var testBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("Test file content"));
            var result = await _firebaseService.UploadDocumentAsync(testBase64, "test-file", ".txt");
            
            return Ok(ApiResponse.Ok("Firebase test completed", new { 
                Success = result.Success, 
                DocumentId = result.DocumentId,
                Url = result.Url,
                ErrorMessage = result.ErrorMessage
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Firebase test failed");
            return StatusCode(500, ApiResponse.Fail($"Firebase test failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Upload a new document
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<DocumentUploadResultDto>> UploadDocument([FromBody] UploadDocumentDto uploadDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            _logger.LogInformation("Starting document upload for user {UserId}, document: {DocumentName}", userId, uploadDto.DocumentName);

            var result = await _documentService.UploadDocumentAsync(uploadDto, userId);

            _logger.LogInformation("Document uploaded successfully. Database ID: {DatabaseId}, Firebase DocumentId: {FirebaseDocumentId}, Url: {Url}", 
                result.Id, result.DocumentId, result.Url);

            return Ok(ApiResponse.Ok("Document Uploaded Successfully", result));

            // return Ok(ApiResponse.Ok(result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error uploading document: {DocumentName}", uploadDto.DocumentName);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading document: {DocumentName}", uploadDto.DocumentName);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred during document upload"));
        }
    }

    /// <summary>
    /// Upload multiple documents via multipart form data
    /// Uploads documents immediately to Firebase
    /// </summary>
    [HttpPost("upload-multipart")]
    [RequestSizeLimit(52428800)] // 50MB limit
    [RequestFormLimits(MultipartBodyLengthLimit = 52428800)]
    public async Task<ActionResult<MultipartUploadResponseDto>> UploadMultipartDocuments([FromForm] IFormFileCollection files)
    {
        try
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(ApiResponse.Fail("No files provided"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            _logger.LogInformation("Starting multipart upload for user {UserId}, {FileCount} files", userId, files.Count);

            var uploadedDocuments = new List<DocumentUploadPendingDto>();

            foreach (var file in files)
            {
                if (file.Length == 0)
                {
                    _logger.LogWarning("Skipping empty file: {FileName}", file.FileName);
                    continue;
                }

                // Read file as base64
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(fileBytes);

                var fileExtension = Path.GetExtension(file.FileName);

                // Upload directly to Firebase
                var uploadResult = await _firebaseService.UploadDocumentAsync(
                    base64String,
                    file.FileName,
                    fileExtension);

                if (!uploadResult.Success)
                {
                    _logger.LogError("Failed to upload file {FileName}: {Error}", file.FileName, uploadResult.ErrorMessage);
                    continue;
                }

                // Create document record with completed status
                var document = new Document
                {
                    DocumentName = file.FileName,
                    DocumentType = file.ContentType,
                    FileExtension = fileExtension,
                    FileSizeBytes = file.Length,
                    DocumentUrl = uploadResult.Url,
                    UploadedBy = userId,
                    Status = DocumentStatus.Completed,
                    CreatedAt = DateTime.UtcNow,
                    UploadedAt = DateTime.UtcNow
                };

                var savedDocument = await _documentRepository.CreateDocumentAsync(document);

                uploadedDocuments.Add(new DocumentUploadPendingDto
                {
                    Id = savedDocument.Id,
                    DocumentName = savedDocument.DocumentName,
                    Status = savedDocument.Status,
                    CreatedAt = savedDocument.CreatedAt
                });

                _logger.LogInformation("Uploaded document {DocumentId} for file {FileName}", 
                    savedDocument.Id, file.FileName);
            }

            var response = new MultipartUploadResponseDto
            {
                Documents = uploadedDocuments,
                Message = $"{uploadedDocuments.Count} document(s) uploaded successfully."
            };

            return Ok(ApiResponse.Ok("Documents uploaded successfully", response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in multipart document upload");
            return StatusCode(500, ApiResponse.Fail($"An error occurred during upload: {ex.Message}"));
        }
    }

    /// <summary>
    /// Get document by database ID (use the 'id' field from upload response, not 'documentId')
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(string id)
    {
        try
        {
            _logger.LogInformation("Retrieving document with ID: {DocumentId}", id);

            var document = await _documentService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning("Document not found: {DocumentId}", id);
                return NotFound(ApiResponse.Fail("Document not found"));
            }

            return Ok(ApiResponse.Ok("Document fetched successfully", document));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error retrieving document: {DocumentId}", id);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving document: {DocumentId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred while retrieving the document"));
        }
    }

    /// <summary>
    /// Get all documents
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<DocumentDto>>> GetAllDocuments()
    {
        try
        {
            _logger.LogInformation("Retrieving all documents");

            var documents = await _documentService.GetAllDocumentsAsync();

            _logger.LogInformation("Retrieved {DocumentCount} documents", documents.Count);
            return Ok(ApiResponse.Ok("Documents retrieved successfully", documents));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error retrieving all documents");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all documents");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred while retrieving documents"));
        }
    }

    /// <summary>
    /// Delete document by ID
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDocument(string id)
    {
        try
        {
            _logger.LogInformation("Deleting document with ID: {DocumentId}", id);

            var result = await _documentService.DeleteDocumentAsync(id);
            if (result)
            {
                _logger.LogInformation("Document deleted successfully: {DocumentId}", id);
                return Ok(ApiResponse.Ok("Document deleted successfully"));
            }

            _logger.LogWarning("Failed to delete document: {DocumentId}", id);
            return BadRequest(ApiResponse.Fail("Failed to delete document"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentId}", id);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting document: {DocumentId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred while deleting the document"));
        }
    }
}
