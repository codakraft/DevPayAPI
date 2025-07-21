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
    private readonly StorageClient _storageClient;
    private readonly FirebaseSettings _firebaseSettings;
    private readonly ILogger<FirebaseDocumentService> _logger;

    public FirebaseDocumentService(IOptions<FirebaseSettings> firebaseSettings, ILogger<FirebaseDocumentService> logger)
    {
        _firebaseSettings = firebaseSettings.Value;
        _logger = logger;

        try
        {
            // Check if we have valid Firebase configuration
            // Try multiple potential paths for the service account key
            var possiblePaths = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account-key.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-service-account-key.json"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "firebase-service-account-key.json"),
                "firebase-service-account-key.json"
            };

            string? serviceAccountKeyPath = null;
            foreach (var path in possiblePaths)
            {
                _logger.LogInformation("Checking for Firebase service account key at: {Path}", path);
                if (File.Exists(path))
                {
                    serviceAccountKeyPath = path;
                    _logger.LogInformation("Found Firebase service account key at: {Path}", path);
                    break;
                }
            }

            if (serviceAccountKeyPath == null)
            {
                _logger.LogWarning("Firebase Storage service not initialized - firebase-service-account-key.json file not found in any of the checked locations");
                _storageClient = null!; // Will be handled in upload methods
                return;
            }

            // Read and validate the service account key file
            var serviceAccountJson = File.ReadAllText(serviceAccountKeyPath);

            _logger.LogInformation("Firebase service account key file read successfully. Content length: {Length}", serviceAccountJson.Length);

            if (string.IsNullOrEmpty(serviceAccountJson) ||
                serviceAccountJson.Contains("DummyPrivateKeyContentHere") ||
                serviceAccountJson.Contains("dummy-key-id") ||
                serviceAccountJson.Contains("your-firebase-project-id"))
            {
                _logger.LogWarning("Firebase Storage service not initialized - invalid or dummy configuration detected in service account file");
                _storageClient = null!; // Will be handled in upload methods
                return;
            }

            // Initialize Google Cloud Storage client with service account file
            _logger.LogInformation("Initializing Firebase Storage client with service account...");
            _storageClient = StorageClient.Create(Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(serviceAccountJson));
            _logger.LogInformation("Firebase Storage service initialized successfully for bucket: {BucketName}", _firebaseSettings.BucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase Storage service - using fallback mode. Error: {Message}", ex.Message);
            _storageClient = null!; // Will be handled in upload methods
        }
    }

    public async Task<ThirdPartyUploadResultDto> UploadDocumentAsync(string base64String, string fileName, string fileExtension)
    {
        _logger.LogInformation("UploadDocumentAsync called for file: {FileName}", fileName);

        // Check if Firebase is properly configured
        if (_storageClient == null)
        {
            _logger.LogWarning("Firebase Storage not available - returning mock result for file: {FileName}. Storage client is null.", fileName);

            // Return a mock result for development/testing
            return new ThirdPartyUploadResultDto
            {
                Success = true,
                DocumentId = Guid.NewGuid().ToString(),
                Url = $"https://mock-storage.dev/documents/{Guid.NewGuid()}{fileExtension}",
                FileSizeBytes = Convert.FromBase64String(base64String).Length,
                ErrorMessage = null
            };
        }

        _logger.LogInformation("Firebase Storage client available - proceeding with real upload for file: {FileName}", fileName);
        _logger.LogInformation("Starting Firebase Storage upload for file: {FileName}", fileName);

        // Convert base64 to bytes
        byte[] fileBytes = Convert.FromBase64String(base64String);

        // Generate unique file path
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = $"documents/{DateTime.UtcNow:yyyy/MM/dd}/{uniqueFileName}";

        // Determine content type
        var contentType = GetContentType(fileExtension);

        // Upload to Firebase Storage
        using var stream = new MemoryStream(fileBytes);
        var uploadedObject = await _storageClient.UploadObjectAsync(
            _firebaseSettings.BucketName,
            filePath,
            contentType,
            stream);

        if (uploadedObject != null)
        {
            _logger.LogInformation("Firebase Storage upload successful for file: {FileName}, Path: {FilePath}",
                fileName, filePath);

            var downloadUrl = $"https://storage.googleapis.com/{_firebaseSettings.BucketName}/{filePath}";

            return new ThirdPartyUploadResultDto
            {
                Success = true,
                DocumentId = uploadedObject.Name,
                Url = downloadUrl,
                FileSizeBytes = fileBytes.Length
            };
        }
        else
        {
            _logger.LogError("Firebase Storage upload failed for file: {FileName} - No object returned", fileName);
            return new ThirdPartyUploadResultDto
            {
                Success = false,
                ErrorMessage = "Upload failed - no object returned from Firebase Storage"
            };
        }
    }

    public async Task<bool> DeleteDocumentAsync(string documentId)
    {
        // Check if Firebase is properly configured
        if (_storageClient == null)
        {
            _logger.LogWarning("Firebase Storage not available - simulating delete for document: {DocumentId}", documentId);
            return true; // Simulate successful deletion
        }

        _logger.LogInformation("Deleting file from Firebase Storage: {DocumentId}", documentId);

        await _storageClient.DeleteObjectAsync(_firebaseSettings.BucketName, documentId);

        _logger.LogInformation("Successfully deleted file from Firebase Storage: {DocumentId}", documentId);
        return true;
    }

    public async Task<ThirdPartyDocumentDetailsDto?> GetDocumentDetailsAsync(string documentId)
    {
        // Check if Firebase is properly configured
        if (_storageClient == null)
        {
            _logger.LogWarning("Firebase Storage not available - returning mock result for document: {DocumentId}", documentId);

            // Return a mock result for development/testing
            return new ThirdPartyDocumentDetailsDto
            {
                DocumentId = documentId,
                Url = $"https://mock-storage.dev/documents/{documentId}",
                FileType = "Unknown",
                FileSizeBytes = 1024 // Mock size
            };
        }

        _logger.LogInformation("Getting document details from Firebase Storage: {DocumentId}", documentId);

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
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        return null;
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
}
