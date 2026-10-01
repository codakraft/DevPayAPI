using LendingSolution.API.Auth;
using LendingSolution.Application.Exceptions;
using LendingSolution.Core.Auth;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize]
public class AdminController(
    IAuthService authService, 
    ICompanyService companyService, 
    ILoanProductService loanProductService,
    ILoanService loanService,
    ILogger<AdminController> logger) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ICompanyService _companyService = companyService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ILoanService _loanService = loanService;
    private readonly ILogger<AdminController> _logger = logger;

    /// <summary>
    /// Gets all admin roles available in the system
    /// </summary>
    /// <returns>List of all admin roles with their IDs and names</returns>
    // [GET]    /api/admin/roles
    [HttpGet("roles")]
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAllRoles()
    {
        try
        {
            var result = await _authService.GetRoles();
            _logger.LogInformation("Roles fetched successfully");
            return Ok(ApiResponse.Ok("Roles fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching roles.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Assigns a role to a user. SuperAdmin can assign any role; company Admins can only
    /// assign company staff roles to non-admin users in their own company.
    /// </summary>
    // [POST]   /api/admin/role/assign
    [HttpPost("role/assign")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto body)
    {
        try
        {
            var result = await _authService.AssignRole(body, User);
            _logger.LogInformation("Role assigned successfully to user {UserId}", body.UserId);
            return Ok(ApiResponse.Ok("Role assigned successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning role.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Removes a role from a user. Same scoping rules as role assignment.
    /// The user's current access tokens stop working immediately.
    /// </summary>
    // [POST]   /api/admin/role/remove
    [HttpPost("role/remove")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<IActionResult> RemoveRole([FromBody] RoleAssignDto body)
    {
        try
        {
            var result = await _authService.RemoveRole(body, User);
            _logger.LogInformation("Role removed successfully from user {UserId}", body.UserId);
            return Ok(ApiResponse.Ok("Role removed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing role.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Deactivates a user: blocks login and signs them out everywhere (access and refresh tokens).
    /// </summary>
    // [POST]   /api/admin/users/{userId}/deactivate
    [HttpPost("users/{userId}/deactivate")]
    [HasPermission(Permissions.Users.Manage)]
    public Task<IActionResult> DeactivateUser(string userId) => SetUserActive(userId, false);

    /// <summary>
    /// Re-activates a previously deactivated user.
    /// </summary>
    // [POST]   /api/admin/users/{userId}/activate
    [HttpPost("users/{userId}/activate")]
    [HasPermission(Permissions.Users.Manage)]
    public Task<IActionResult> ActivateUser(string userId) => SetUserActive(userId, true);

    /// <summary>
    /// Updates a user's first and last name. Same scoping rules as role changes: non-SuperAdmins can
    /// only edit non-admin users in their own company, and nobody can edit themselves here.
    /// </summary>
    // [PUT]    /api/admin/users/{userId}
    [HttpPut("users/{userId}")]
    [HasPermission(Permissions.Users.Manage)]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequestDto body)
    {
        try
        {
            var result = await _authService.UpdateUserAsync(userId, body, User);
            _logger.LogInformation("User {UserId} updated", userId);
            return Ok(ApiResponse.Ok("User updated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    private async Task<IActionResult> SetUserActive(string userId, bool isActive)
    {
        try
        {
            var result = await _authService.SetUserActiveAsync(userId, isActive, User);
            _logger.LogInformation("User {UserId} active status set to {IsActive}", userId, isActive);
            return Ok(ApiResponse.Ok(isActive ? "User activated successfully" : "User deactivated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user status.");
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
            _logger.LogInformation("Super admin dashboard fetched successfully");
            return Ok(ApiResponse.Ok("Super admin dashboard fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
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
            _logger.LogInformation("Admin list fetched successfully with filters");
            return Ok(ApiResponse.Ok("Admin list fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching admin list.");
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
            _logger.LogInformation("All loan products fetched successfully with filters");
            return Ok(ApiResponse.Ok("Loan products fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all loan products.");
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
            _logger.LogInformation("All loans fetched successfully with filters");
            return Ok(ApiResponse.Ok("Loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all loans.");
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
            _logger.LogInformation("Company {CompanyId} loans fetched successfully with filters", companyId);
            return Ok(ApiResponse.Ok("Company loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company loans.");
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
            _logger.LogInformation("Loan {LoanId} fetched successfully", loanId);
            return Ok(ApiResponse.Ok("Loan fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loan by ID.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

}

