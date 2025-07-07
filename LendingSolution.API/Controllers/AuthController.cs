using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AuthController(
    IAuthService authService,
    ICompanyService companyService,
    ILoanProductService loanProductService,
    ILogger<AuthController> logger) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ICompanyService _companyService = companyService;
    private readonly ILogger<AuthController> _logger = logger;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto body)
    {
        try
        {
            var result = await _authService.AdminLogin(body);
            return Ok(ApiResponse.Ok("Login successful", result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred: " + ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpPost("sa/create")]
    public async Task<IActionResult> CreateSuperAdmin([FromBody] CreateSuperAdminRequestDto body)
    {
        try
        {
            var result = await _authService.CreateSuperAdmin(body);
            return Ok(result);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error occurred while creating super admin.");
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while creating super admin.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("sa/admin/create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequestDto body)
    {
        try
        {
            var result = await _authService.CreateAdmin(body);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating admin.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var result = await _authService.RefreshToken(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing token.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.RevokeToken(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while revoking token.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.Logout(userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while logging out.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}

