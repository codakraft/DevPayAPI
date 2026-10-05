using LendingSolution.Core.Models;

namespace LendingSolution.Core.Dtos;

public class PagedAuditLogListDto
{
    public List<AuditLog> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
