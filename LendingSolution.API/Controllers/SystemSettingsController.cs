using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/system")]
[Authorize(Roles = "SuperAdmin")]
public class SystemSettingsController : ControllerBase
{
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly ILogger<SystemSettingsController> _logger;

    public SystemSettingsController(
        ISystemSettingsService systemSettingsService,
        ILogger<SystemSettingsController> logger)
    {
        _systemSettingsService = systemSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all system settings (SuperAdmin only)
    /// </summary>
    /// <returns>List of all system settings</returns>
    [HttpGet("settings")]
    public async Task<IActionResult> GetAllSystemSettings()
    {
        try
        {
            var result = await _systemSettingsService.GetAllSettingsAsync();
            _logger.LogInformation("System settings fetched successfully");
            return Ok(ApiResponse.Ok("System settings fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching system settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get active system settings (SuperAdmin only)
    /// </summary>
    /// <returns>Active system settings</returns>
    [HttpGet("settings/active")]
    public async Task<IActionResult> GetActiveSystemSettings()
    {
        try
        {
            var result = await _systemSettingsService.GetActiveSettingsAsync();
            _logger.LogInformation("Active system settings fetched successfully");
            return Ok(ApiResponse.Ok("Active system settings fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching active system settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get system settings by ID (SuperAdmin only)
    /// </summary>
    /// <param name="id">Settings ID</param>
    /// <returns>System settings details</returns>
    [HttpGet("settings/{id}")]
    public async Task<IActionResult> GetSystemSettingsById(Guid id)
    {
        try
        {
            var result = await _systemSettingsService.GetSettingsByIdAsync(id);
            _logger.LogInformation("System settings {SettingsId} fetched successfully", id);
            return Ok(ApiResponse.Ok("System settings fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching system settings {SettingsId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Create new system settings (SuperAdmin only)
    /// </summary>
    /// <param name="settingsDto">System settings data</param>
    /// <returns>Created system settings</returns>
    [HttpPost("settings")]
    public async Task<IActionResult> CreateSystemSettings([FromBody] UpdateSystemSettingsDto settingsDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _systemSettingsService.CreateSettingsAsync(settingsDto, userId);
            
            _logger.LogInformation("System settings created successfully by user {UserId}", userId);
            return Ok(ApiResponse.Ok("System settings created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating system settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Update system settings (SuperAdmin only) - Partial update of active settings
    /// </summary>
    /// <param name="settingsDto">System settings data to update (only provided fields will be updated)</param>
    /// <returns>Updated system settings</returns>
    [HttpPatch("settings")]
    public async Task<IActionResult> PatchSystemSettings([FromBody] PatchSystemSettingsDto settingsDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _systemSettingsService.PatchActiveSettingsAsync(settingsDto, userId);
            
            _logger.LogInformation("System settings updated successfully by user {UserId}", userId);
            return Ok(ApiResponse.Ok("System settings updated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating system settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}
