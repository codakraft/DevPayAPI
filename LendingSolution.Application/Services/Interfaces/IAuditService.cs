using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string category, string? userId = null, string? userEmail = null, 
        string? entityType = null, string? entityId = null, Guid? companyId = null, 
        string? details = null, decimal? amount = null, decimal? oldBalance = null, 
        decimal? newBalance = null, string? ipAddress = null, bool isSuccess = true, 
        string? errorMessage = null);
    
    Task<PagedAuditLogListDto> GetLogsAsync(string? category = null, Guid? companyId = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
}
