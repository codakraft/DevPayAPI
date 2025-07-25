using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/v1/settings")]
[Authorize(Roles = "SuperAdmin")]       
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// Get application settings (SuperAdmin only)
    /// </summary>
    /// <returns>Application settings</returns>
    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var result = await _settingsService.GetSettingsAsync();
            _logger.LogInformation("Settings fetched successfully");
            return Ok(ApiResponse.Ok("Settings fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Update application settings (SuperAdmin only)
    /// </summary>
    /// <param name="settingsDto">Updated settings data</param>
    /// <returns>Updated settings</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsDto settingsDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";
            var result = await _settingsService.UpdateSettingsAsync(settingsDto, userId);
            _logger.LogInformation("Settings updated successfully");
            return Ok(ApiResponse.Ok("Settings updated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating settings.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}
