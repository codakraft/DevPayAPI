using LendingSolution.API.Auth;
using LendingSolution.Core.Auth;
using Microsoft.AspNetCore.Mvc;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Models;
using Asp.Versioning;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[HasPermission(Permissions.Audit.View)]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    /// <summary>
    /// Get audit logs with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AuditLog>>> GetLogs(
        [FromQuery] string? category = null,
        [FromQuery] Guid? companyId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        if (!User.IsSuperAdmin())
        {
            // Non-SuperAdmins only ever see their own company's audit trail
            companyId = User.GetCompanyId();
            if (companyId is null)
            {
                return Forbid();
            }
        }

        var logs = await _auditService.GetLogsAsync(category, companyId, fromDate, toDate, page, pageSize);
        return Ok(logs);
    }
}
