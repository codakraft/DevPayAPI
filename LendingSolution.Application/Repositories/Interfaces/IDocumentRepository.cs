using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IDocumentRepository
{
    Task<Document> CreateDocumentAsync(Document document);
    Task<Document?> GetDocumentByIdAsync(string id);
    Task<List<Document>> GetAllDocumentsAsync();
    Task<List<Document>> GetDocumentsByTypeAsync(string documentType);
    Task<List<Document>> GetDocumentsByStatusAsync(DocumentStatus status);
    Task<bool> UpdateDocumentAsync(Document document);
    Task<bool> DeleteDocumentAsync(string id);
}
