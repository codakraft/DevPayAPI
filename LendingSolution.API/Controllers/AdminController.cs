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
    ILogger<AdminController> logger) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ICompanyService _companyService = companyService;
    private readonly IAdminSettingsService _adminSettingsService = adminSettingsService;
    private readonly IApprovalService _approvalService = approvalService;
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

}

