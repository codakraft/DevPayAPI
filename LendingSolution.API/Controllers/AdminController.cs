using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
public class AdminController : Controller
{
    private readonly IAuthService _authService;
    private readonly ICompanyService _companyService;
    public AdminController(IAuthService authService, ICompanyService companySErvice)
    {
        _authService = authService;
        _companyService = companySErvice;
    }

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

    [HttpPost("create-product")]
    public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDto body)
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

}
