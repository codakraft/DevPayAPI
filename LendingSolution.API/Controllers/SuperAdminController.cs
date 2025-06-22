using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
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
    [HttpPost("create-super-admin")]
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

    [HttpPost("create-company")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = await _companyService.CreateCompany(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }



    // company/add-admin

    // company/remove-admin

    // company/deactivate

    // company/activate

}
