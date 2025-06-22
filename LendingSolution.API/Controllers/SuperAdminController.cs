using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[Authorize(Roles = "SuperAdmin")]
[ApiController]
[Route("api/sa")]
[Authorize]
public class SuperAdminController(IAuthService authService, ICompanyService companySErvice, ILoanProductService loanProductService) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ICompanyService _companyService = companySErvice;

    // create
    [AllowAnonymous]
    [HttpPost("super-admin/create")]
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

    // company/create-admin
    [HttpPost("admin/create")]
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

    [HttpPost("company/create")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
               ApiResponse.Fail("Invalid model state")
           );
        }

        var result = await _companyService.CreateCompany(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // company/activate
    [HttpGet("company/deactivate/{companyId}")]
    public async Task<IActionResult> DeactivateCompany([FromRoute] Guid companyId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model state")
            );
        }

        var result = await _companyService.Deactivate(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // company/activate
    [HttpGet("company/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid companyId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model state")
            );
        }

        var result = await _companyService.Activate(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    // company/deactivate-admin

    // company/remove-admin

    // company/deactivate


}
