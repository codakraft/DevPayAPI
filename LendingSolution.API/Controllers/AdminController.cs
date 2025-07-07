using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController(
    IAuthService authService, 
    ICompanyService companyService, 
    IAdminSettingsService adminSettingsService,
    IApprovalService approvalService,
    ILoanProductService loanProductService,
    ILoanService loanService,
    ILogger<AdminController> logger) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ICompanyService _companyService = companyService;
    private readonly IAdminSettingsService _adminSettingsService = adminSettingsService;
    private readonly IApprovalService _approvalService = approvalService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ILoanService _loanService = loanService;
    private readonly ILogger<AdminController> _logger = logger;

    /// <summary>
    /// Gets all admin roles available in the system
    /// </summary>
    /// <returns>List of all admin roles with their IDs and names</returns>
    // [GET]    /api/admin/roles
    [HttpGet("roles")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var result = await _authService.GetRoles();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all roles.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/admin/users/{id}/assign-role
    [HttpPost("role/assign")]
    public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto body)
    {
        try
        {
            var result = await _authService.AssignRole(body);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role to user.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/admin/settings
    [HttpGet("settings")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAllSettings()
    {
        try
        {
            var result = await _adminSettingsService.GetAllSettingsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching admin settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/admin/settings
    [HttpPost("settings")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateSettings([FromBody] UpdateAdminSettingsDto settingsDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _adminSettingsService.CreateSettingAsync(settingsDto, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating admin settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [PUT]    /api/admin/settings
    [HttpPut("settings/{id}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateSettings(string id, [FromBody] UpdateAdminSettingsDto settingsDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _adminSettingsService.UpdateSettingAsync(id, settingsDto, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating admin settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/admin/approvals/pending
    [HttpGet("approvals/pending")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        try
        {
            var result = await _approvalService.GetPendingApprovalsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching pending approvals.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/admin/approvals/{id}/approve
    [HttpPost("approvals/{id}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> ApproveRequest(string id, [FromBody] ProcessApprovalDto? processDto = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _approvalService.ApproveRequestAsync(id, userId, processDto?.Reason);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving request.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/admin/approvals/{id}/reject
    [HttpPost("approvals/{id}/reject")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RejectRequest(string id, [FromBody] ProcessApprovalDto? processDto = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _approvalService.RejectRequestAsync(id, userId, processDto?.Reason);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting request.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets comprehensive system-wide dashboard for super admin
    /// </summary>
    /// <returns>Complete system analytics across all companies</returns>
    // [GET] /api/admin/super-dashboard
    [HttpGet("super-dashboard")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetSuperAdminDashboard()
    {
        try
        {
            var result = await _authService.GetSuperAdminDashboardAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching super admin dashboard.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets a paginated list of admins with filtering and search capabilities
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering admins</param>
    /// <returns>Paginated list of admins with their details</returns>
    // [GET] /api/admin/list
    [HttpGet("list")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAdminList([FromQuery] AdminFilterDto filter)
    {
        try
        {
            var result = await _authService.GetAdminListAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching admin list with filters: {@Filter}", filter);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all loan products across all companies with filtering and search (SuperAdmin only)
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering loan products</param>
    /// <returns>Paginated list of loan products with company information</returns>
    // [GET] /api/admin/loan-products
    [HttpGet("loan-products")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllLoanProducts([FromQuery] LoanProductFilterDto filter)
    {
        try
        {
            var result = await _loanProductService.GetAllLoanProductsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all loan products with filters: {@Filter}", filter);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all loans across all companies with filtering and search (SuperAdmin only)
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering loans</param>
    /// <returns>Paginated list of loans with complete information</returns>
    // [GET] /api/admin/loans
    [HttpGet("loans")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllLoans([FromQuery] LoanFilterDto filter)
    {
        try
        {
            var result = await _loanService.GetAllLoansAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all loans with filters: {@Filter}", filter);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all loans for a specific company (SuperAdmin only)
    /// </summary>
    /// <param name="companyId">Company ID to filter loans</param>
    /// <param name="filter">Filter parameters for searching and filtering loans</param>
    /// <returns>Paginated list of loans for the specified company</returns>
    // [GET] /api/admin/loans/company/{companyId}
    [HttpGet("loans/company/{companyId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCompanyLoans(Guid companyId, [FromQuery] LoanFilterDto filter)
    {
        try
        {
            var result = await _loanService.GetCompanyLoansAsync(companyId, filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company loans with filters: {@Filter}", filter);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets a specific loan by ID (SuperAdmin only)
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <returns>Loan details with complete information</returns>
    // [GET] /api/admin/loans/{loanId}
    [HttpGet("loans/{loanId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetLoanById(Guid loanId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _loanService.GetLoanByIdAsync(loanId, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loan {LoanId}", loanId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

}

