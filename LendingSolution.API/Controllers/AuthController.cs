using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AuthController(IAuthService authService, ICompanyService companySErvice, ILoanProductService loanProductService) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ICompanyService _companyService = companySErvice;

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
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model State"));
        }

        var result = await _authService.CreateSuperAdmin(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "superAdmin")]
    [HttpPost("sa/admin/create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model state")
            );
        }

        var result = await _authService.CreateAdmin(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // [GET]  /api/auth/roles
    // [GET]  /api/auth/me
    // [POST] /api/auth/logout
    // [POST] /api/auth/refresh-token

}