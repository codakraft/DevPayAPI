using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LendingSolution.API.Filters;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
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
            var result = await _authService.AdminLoginWithMfaAsync(body);
            _logger.LogInformation("Admin MFA initiated for: {Email}", body.Email);
            return Ok(ApiResponse.Ok("OTP sent to your email", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during admin login.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [AllowAnonymous]
    [HttpPost("verify-login")]
    public async Task<IActionResult> VerifyLogin([FromBody] VerifyAdminLoginRequestDto body)
    {
        try
        {
            var result = await _authService.VerifyAdminLoginOtpAsync(body);
            _logger.LogInformation("Admin login verified successfully");
            return Ok(ApiResponse.Ok("Login successful", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode).WithData(ex.Details));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during OTP verification.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 1 of resetting a forgotten password: emails a 6-digit reset code.
    /// Always returns the same response so it can't be used to discover accounts.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] AdminForgotPasswordRequestDto body)
    {
        try
        {
            await _authService.RequestAdminPasswordResetAsync(body.Email);
            return Ok(ApiResponse.Ok("If an account exists for this email, a reset code has been sent."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while requesting a password reset.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 2 of resetting a forgotten password: checks the emailed code and sets the new password.
    /// Signs the user out everywhere; they log in again with the new password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordRequestDto body)
    {
        try
        {
            await _authService.ResetAdminPasswordAsync(body);
            _logger.LogInformation("Password reset for {Email}", body.Email);
            return Ok(ApiResponse.Ok("Password reset successfully. Please log in with your new password."));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode).WithData(ex.Details));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting password.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [AllowAnonymous]
    [HttpPost("sa/create")]
    public async Task<IActionResult> CreateSuperAdmin([FromBody] CreateSuperAdminRequestDto body)
    {
        try
        {
            var result = await _authService.CreateSuperAdmin(
                body, User.FindFirstValue(ClaimTypes.NameIdentifier), User.IsInRole("SuperAdmin"));
            _logger.LogInformation("Super admin created successfully: {Email}", body.Email);
            return Ok(ApiResponse.Ok("Super admin created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating super admin.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("sa/admin/create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequestDto body)
    {
        try
        {
            var result = await _authService.CreateAdmin(body, User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _logger.LogInformation("Admin created successfully: {Email}", body.Email);
            return Ok(ApiResponse.Ok("Admin created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating admin.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [AllowWhilePasswordChangeRequired]
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var result = await _authService.RefreshToken(request);
            _logger.LogInformation("Token refreshed successfully");
            return Ok(ApiResponse.Ok("Token refreshed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail($"Failed to refresh token: {ex.Message}").WithCode(ex.ErrorCode));
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.RevokeToken(request, userId);
            _logger.LogInformation("User {UserId} revoked a token", userId);
            return Ok(ApiResponse.Ok("JWT revoked successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while revoking token.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [AllowWhilePasswordChangeRequired]
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.Logout(userId);
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return Ok(ApiResponse.Ok("Logout successful"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while logging out.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Change password. Also clears the first-login password change requirement.
    /// </summary>
    [AllowWhilePasswordChangeRequired]
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto body)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var tokens = await _authService.ChangePasswordAsync(body, userId);
            _logger.LogInformation("Password changed successfully for user {UserId}", userId);
            return Ok(ApiResponse.Ok("Password changed successfully", tokens));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}

