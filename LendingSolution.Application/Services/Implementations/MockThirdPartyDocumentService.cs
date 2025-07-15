using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Mock implementation of third-party document service for demo/testing purposes
/// </summary>
public class MockThirdPartyDocumentService : IThirdPartyDocumentService
{
    private readonly ILogger<MockThirdPartyDocumentService> _logger;
    
    // In-memory storage for demo purposes
    private static readonly Dictionary<string, ThirdPartyDocumentDetailsDto> _mockStorage = new();

    public MockThirdPartyDocumentService(ILogger<MockThirdPartyDocumentService> logger)
    {
        _logger = logger;
    }

    public async Task<ThirdPartyUploadResultDto> UploadDocumentAsync(string base64String, string fileName, string fileExtension)
    {
        try
        {
            _logger.LogInformation("Mock upload started for file: {FileName}", fileName);

            // Simulate processing delay
            await Task.Delay(100);

            // Generate mock document ID
            var documentId = $"mock_doc_{Guid.NewGuid():N}";
            
            // Calculate approximate file size from base64 string
            var fileSizeBytes = (long)(base64String.Length * 0.75); // Approximate size after base64 decoding
            
            // Generate mock URL
            var mockUrl = $"https://mockstorage.example.com/documents/{documentId}{fileExtension}";

            // Store in mock storage
            var documentDetails = new ThirdPartyDocumentDetailsDto
            {
                DocumentId = documentId,
                Url = mockUrl,
                FileType = GetFileTypeFromExtension(fileExtension),
                FileSizeBytes = fileSizeBytes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _mockStorage[documentId] = documentDetails;

            _logger.LogInformation("Mock upload completed for file: {FileName}, DocumentId: {DocumentId}", fileName, documentId);

            return new ThirdPartyUploadResultDto
            {
                Success = true,
                DocumentId = documentId,
                Url = mockUrl,
                FileSizeBytes = fileSizeBytes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mock document upload for file: {FileName}", fileName);
            
            return new ThirdPartyUploadResultDto
            {
                Success = false,
                ErrorMessage = $"Mock upload failed: {ex.Message}"
            };
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            _logger.LogInformation("Mock delete started for DocumentId: {DocumentId}", documentId);

            // Simulate processing delay
            await Task.Delay(50);

            // Remove from mock storage
            var removed = _mockStorage.Remove(documentId);

            _logger.LogInformation("Mock delete completed for DocumentId: {DocumentId}, Success: {Success}", documentId, removed);

            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mock document deletion for DocumentId: {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<ThirdPartyDocumentDetailsDto?> GetDocumentDetailsAsync(string documentId)
    {
        try
        {
            _logger.LogInformation("Mock get document details for DocumentId: {DocumentId}", documentId);

            // Simulate processing delay
            await Task.Delay(50);

            // Get from mock storage
            var found = _mockStorage.TryGetValue(documentId, out var documentDetails);

            _logger.LogInformation("Mock get document details completed for DocumentId: {DocumentId}, Found: {Found}", documentId, found);

            return documentDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during mock document details retrieval for DocumentId: {DocumentId}", documentId);
            return null;
        }
    }

    private static string GetFileTypeFromExtension(string? fileExtension)
    {
        if (string.IsNullOrEmpty(fileExtension))
            return "Unknown";

        var extension = fileExtension.TrimStart('.').ToLowerInvariant();
        
        return extension switch
        {
            "pdf" => "PDF",
            "jpg" or "jpeg" => "JPEG Image",
            "png" => "PNG Image",
            "gif" => "GIF Image",
            "doc" or "docx" => "Word Document",
            "xls" or "xlsx" => "Excel Spreadsheet",
            "txt" => "Text File",
            "zip" or "rar" => "Archive",
            _ => "Unknown"
        };
    }
}
