using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Interface for third-party document upload and management service
/// </summary>
public interface IThirdPartyDocumentService
{
    /// <summary>
    /// Upload a document to third-party storage service
    /// </summary>
    /// <param name="base64String">Base64 encoded file content</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileExtension">File extension</param>
    /// <returns>Upload result with URL and document ID</returns>
    Task<ThirdPartyUploadResultDto> UploadDocumentAsync(string base64String, string fileName, string fileExtension);

    /// <summary>
    /// Delete a document from third-party storage service
    /// </summary>
    /// <param name="documentId">Third-party document ID</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteDocumentAsync(string documentId);

    /// <summary>
    /// Get document details from third-party service
    /// </summary>
    /// <param name="documentId">Third-party document ID</param>
    /// <returns>Document details or null if not found</returns>
    Task<ThirdPartyDocumentDetailsDto?> GetDocumentDetailsAsync(string documentId);
}
