using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILoanProductService _loanProductService;
    private readonly ICompanyService _companyService;
    public AdminController(IAuthService authService, ICompanyService companySErvice, ILoanProductService loanProductService)
    {
        _loanProductService = loanProductService;
        _authService = authService;
        _companyService = companySErvice;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto body)
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

        var result = await _authService.AdminLogin(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("create-product")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateLoanProductRequestDto body)
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

        var result = await _loanProductService.CreateLoanProduct(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpGet("get-loan-products/{companyId}")]
    public async Task<IActionResult> GetLoanProductsByCmopanyId([FromRoute] Guid companyId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model State")
            );
        }

        var result = await _loanProductService.GetLoanProductsByCompanyId(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

}
