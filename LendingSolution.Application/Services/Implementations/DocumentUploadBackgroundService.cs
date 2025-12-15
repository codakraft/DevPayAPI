using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

public class DocumentUploadBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentUploadBackgroundService> _logger;

    public DocumentUploadBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DocumentUploadBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Document Upload Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingUploadsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Document Upload Background Service");
            }

            // Wait 5 seconds before checking again
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Document Upload Background Service stopped");
    }

    private async Task ProcessPendingUploadsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var documentRepository = scope.ServiceProvider.GetRequiredService<IDocumentRepository>();
        var thirdPartyService = scope.ServiceProvider.GetRequiredService<IThirdPartyDocumentService>();

        // Get all pending documents
        var pendingDocuments = await documentRepository.GetDocumentsByStatusAsync(DocumentStatus.Pending);

        foreach (var document in pendingDocuments)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Processing upload for document {DocumentId}: {DocumentName}", 
                    document.Id, document.DocumentName);

                // Check if temp file exists
                if (string.IsNullOrEmpty(document.TempFilePath) || !File.Exists(document.TempFilePath))
                {
                    _logger.LogWarning("Temp file not found for document {DocumentId}: {TempFilePath}", 
                        document.Id, document.TempFilePath);
                    
                    document.Status = DocumentStatus.Failed;
                    document.ErrorMessage = "Temporary file not found";
                    await documentRepository.UpdateDocumentAsync(document);
                    continue;
                }

                // Read file as base64
                var fileBytes = await File.ReadAllBytesAsync(document.TempFilePath, stoppingToken);
                var base64String = Convert.ToBase64String(fileBytes);

                // Upload to third-party service
                var uploadResult = await thirdPartyService.UploadDocumentAsync(
                    base64String,
                    document.DocumentName,
                    document.FileExtension ?? string.Empty);

                if (uploadResult.Success)
                {
                    document.Status = DocumentStatus.Completed;
                    document.DocumentUrl = uploadResult.Url;
                    document.UploadedAt = DateTime.UtcNow;
                    document.ErrorMessage = null;

                    _logger.LogInformation("Successfully uploaded document {DocumentId} to {Url}", 
                        document.Id, uploadResult.Url);
                }
                else
                {
                    document.Status = DocumentStatus.Failed;
                    document.ErrorMessage = uploadResult.ErrorMessage ?? "Upload failed";

                    _logger.LogError("Failed to upload document {DocumentId}: {ErrorMessage}", 
                        document.Id, uploadResult.ErrorMessage);
                }

                await documentRepository.UpdateDocumentAsync(document);

                // Clean up temp file
                try
                {
                    if (File.Exists(document.TempFilePath))
                    {
                        File.Delete(document.TempFilePath);
                        _logger.LogInformation("Deleted temp file: {TempFilePath}", document.TempFilePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {TempFilePath}", document.TempFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing document {DocumentId}", document.Id);
                
                document.Status = DocumentStatus.Failed;
                document.ErrorMessage = $"Processing error: {ex.Message}";
                await documentRepository.UpdateDocumentAsync(document);
            }
        }
    }
}
