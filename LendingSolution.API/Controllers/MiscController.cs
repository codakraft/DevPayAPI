using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/misc")]
public class MiscController(IAuthService authService, ILogger<MiscController> logger) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<MiscController> _logger = logger;

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            var roles = await _authService.GetRoles();
            return Ok(ApiResponse.Ok("roles fetched successfully", roles));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching roles.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}