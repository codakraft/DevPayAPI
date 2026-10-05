using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(string action, string category, string? userId = null, 
        string? userEmail = null, string? entityType = null, string? entityId = null, 
        Guid? companyId = null, string? details = null, decimal? amount = null, 
        decimal? oldBalance = null, decimal? newBalance = null, string? ipAddress = null, 
        bool isSuccess = true, string? errorMessage = null)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Action = action,
            Category = category,
            UserId = userId,
            UserEmail = userEmail,
            EntityType = entityType,
            EntityId = entityId,
            CompanyId = companyId,
            Details = details,
            Amount = amount,
            OldBalance = oldBalance,
            NewBalance = newBalance,
            IpAddress = ipAddress,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedAuditLogListDto> GetLogsAsync(string? category = null, Guid? companyId = null, 
        DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(a => a.Category == category);

        if (companyId.HasValue)
            query = query.Where(a => a.CompanyId == companyId);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedAuditLogListDto
        {
            Logs = logs,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }
}
