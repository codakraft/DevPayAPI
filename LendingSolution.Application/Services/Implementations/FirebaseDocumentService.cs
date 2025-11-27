using Google.Cloud.Storage.V1;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Firebase Storage implementation of third-party document service
/// </summary>
public class FirebaseDocumentService : IThirdPartyDocumentService
{
    private readonly StorageClient? _storageClient;
    private readonly FirebaseSettings _firebaseSettings;
    private readonly ILogger<FirebaseDocumentService> _logger;
    private readonly bool _isServiceAvailable;

    public FirebaseDocumentService(IOptions<FirebaseSettings> firebaseSettings, ILogger<FirebaseDocumentService> logger)
    {
        _firebaseSettings = firebaseSettings.Value;
        _logger = logger;

        try
        {
            _logger.LogInformation("Initializing Firebase Document Service...");

            // Try to find the service account key file
            var serviceAccountKeyPath = FindServiceAccountKeyFile();
            
            if (serviceAccountKeyPath == null)
            {
                _logger.LogWarning("Firebase service account key not found. Service will operate in mock mode.");
                _storageClient = null;
                _isServiceAvailable = false;
                return;
            }

            // Read and validate the service account key
            var serviceAccountJson = File.ReadAllText(serviceAccountKeyPath);
            
            if (IsValidServiceAccountKey(serviceAccountJson))
            {
                // Initialize Firebase Storage client
                var credential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(serviceAccountJson);
                _storageClient = StorageClient.Create(credential);
                _isServiceAvailable = true;
                
                _logger.LogInformation("Firebase Storage Service initialized successfully for bucket: {BucketName}", 
                    _firebaseSettings.BucketName);
            }
            else
            {
                _logger.LogWarning("Invalid Firebase service account key detected. Service will operate in mock mode.");
                _storageClient = null;
                _isServiceAvailable = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase Storage Service: {Message}", ex.Message);
            _storageClient = null;
            _isServiceAvailable = false;
        }
    }

    public async Task<ThirdPartyUploadResultDto> UploadDocumentAsync(string base64String, string fileName, string fileExtension)
    {
        try
        {
            _logger.LogInformation("Starting document upload for file: {FileName}", fileName);

            if (!_isServiceAvailable || _storageClient == null)
            {
                _logger.LogWarning("Firebase Storage not available - returning mock response for: {FileName}", fileName);
                return CreateMockUploadResult(base64String, fileName, fileExtension);
            }

            // Validate base64 content
            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64String);
            }
            catch (FormatException)
            {
                _logger.LogInformation("Invalid base64 content")
                throw new ArgumentException("Invalid base64 content provided");
            }

            // Generate file info
            var contentType = GetContentType(fileExtension);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = $"documents/{DateTime.UtcNow:yyyy/MM/dd}/{uniqueFileName}";

            // Upload to Firebase Storage
            using var stream = new MemoryStream(fileBytes);
            
            var uploadedObject = await _storageClient.UploadObjectAsync(
                _firebaseSettings.BucketName,
                filePath,
                contentType,
                stream);

            if (uploadedObject != null)
            {
                var downloadUrl = $"https://storage.googleapis.com/{_firebaseSettings.BucketName}/{filePath}";
                
                _logger.LogInformation("Successfully uploaded file: {FileName} to path: {FilePath}", 
                    fileName, filePath);

                return new ThirdPartyUploadResultDto
                {
                    Success = true,
                    DocumentId = uploadedObject.Name,
                    Url = downloadUrl,
                    FileSizeBytes = fileBytes.Length,
                    ErrorMessage = null
                };
            }
            else
            {
                throw new InvalidOperationException("Upload failed - no object returned from Firebase Storage");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document: {FileName}", fileName);
            return new ThirdPartyUploadResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        try
        {
            if (!_isServiceAvailable || _storageClient == null)
            {
                _logger.LogWarning("Firebase Storage not available - simulating delete for: {DocumentId}", documentId);
                return true; // Simulate successful deletion
            }

            _logger.LogInformation("Deleting document: {DocumentId}", documentId);

            await _storageClient.DeleteObjectAsync(_firebaseSettings.BucketName, documentId);
            
            _logger.LogInformation("Successfully deleted document: {DocumentId}", documentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document: {DocumentId}", documentId);
            return false;
        }
    }

    public async Task<ThirdPartyDocumentDetailsDto?> GetDocumentDetailsAsync(string documentId)
    {
        try
        {
            if (!_isServiceAvailable || _storageClient == null)
            {
                _logger.LogWarning("Firebase Storage not available - returning mock response for: {DocumentId}", documentId);
                return CreateMockDocumentDetails(documentId);
            }

            _logger.LogInformation("Retrieving document details for: {DocumentId}", documentId);

            var googleObject = await _storageClient.GetObjectAsync(_firebaseSettings.BucketName, documentId);

            if (googleObject != null)
            {
                var downloadUrl = $"https://storage.googleapis.com/{_firebaseSettings.BucketName}/{documentId}";
                
                return new ThirdPartyDocumentDetailsDto
                {
                    DocumentId = googleObject.Name,
                    Url = downloadUrl,
                    FileType = googleObject.ContentType,
                    FileSizeBytes = (long?)googleObject.Size,
                    CreatedAt = googleObject.TimeCreatedDateTimeOffset?.DateTime ?? DateTime.UtcNow,
                    IsActive = true
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document details: {DocumentId}", documentId);
            return null;
        }
    }

    private static string GetContentType(string fileExtension)
    {
        return fileExtension.ToLower() switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            _ => "application/octet-stream"
        };
    }

    #region Private Helper Methods

    private string? FindServiceAccountKeyFile()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account-key.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-service-account-key.json"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "firebase-service-account-key.json"),
            "firebase-service-account-key.json"
        };

        foreach (var path in possiblePaths)
        {
            _logger.LogDebug("Checking for Firebase service account key at: {Path}", path);
            if (File.Exists(path))
            {
                _logger.LogInformation("Found Firebase service account key at: {Path}", path);
                return path;
            }
        }

        _logger.LogWarning("Firebase service account key not found in any checked locations");
        return null;
    }

    private bool IsValidServiceAccountKey(string serviceAccountJson)
    {
        if (string.IsNullOrWhiteSpace(serviceAccountJson))
            return false;

        // Check for dummy/template content
        var invalidIndicators = new[]
        {
            "DummyPrivateKeyContentHere",
            "dummy-key-id",
            "your-firebase-project-id",
            "YOUR_PRIVATE_KEY_CONTENT_HERE"
        };

        return !invalidIndicators.Any(indicator => 
            serviceAccountJson.Contains(indicator, StringComparison.OrdinalIgnoreCase));
    }

    private ThirdPartyUploadResultDto CreateMockUploadResult(string base64String, string fileName, string fileExtension)
    {
        var documentId = Guid.NewGuid().ToString();
        
        return new ThirdPartyUploadResultDto
        {
            Success = true,
            DocumentId = documentId,
            Url = $"https://mock-storage.dev/documents/{documentId}{fileExtension}",
            FileSizeBytes = Convert.FromBase64String(base64String).Length,
            ErrorMessage = null
        };
    }

    private ThirdPartyDocumentDetailsDto CreateMockDocumentDetails(string documentId)
    {
        return new ThirdPartyDocumentDetailsDto
        {
            DocumentId = documentId,
            Url = $"https://mock-storage.dev/documents/{documentId}",
            FileType = "application/pdf",
            FileSizeBytes = 1024,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            IsActive = true
        };
    }

    #endregion
}
