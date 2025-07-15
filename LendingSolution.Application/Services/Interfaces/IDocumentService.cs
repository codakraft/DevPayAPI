using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IDocumentService
{
    Task<DocumentUploadResultDto> UploadDocumentAsync(UploadDocumentDto uploadDto, string userId);
    Task<DocumentDto?> GetDocumentByIdAsync(string id);
    Task<List<DocumentDto>> GetAllDocumentsAsync();
    Task<List<DocumentDto>> GetDocumentsByTypeAsync(string documentType);
    Task<bool> DeleteDocumentAsync(string id);
}
