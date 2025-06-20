using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
public class UserController : Controller
{
    private readonly IAuthService _authService;
    // private readonly ICompanyUserService _companyUserService;
    private readonly ILoanProductService _loanProductService;
    private readonly ILoanService _loanService;
    public UserController(IAuthService authService, ILoanService loanService, ILoanProductService loanProductService)
    {
        _authService = authService;
        // _companyUserService = companyUserSer;
        _loanService = loanService;
        _loanProductService = loanProductService;
    }

    [HttpPost("onboarding")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto body)
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

        var result = await _authService.Register(body);


        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
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

        var result = await _authService.Login(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequestDto body)
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

        var result = _authService.VerifyOtp(body);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("save-personal-details")]
    public async Task<IActionResult> SavePersonalDetails([FromBody] SavePersonalDetailsRequestDto body)
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

        var result = await _authService.SavePersonalDetails(body, User);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("salary-history-review")]
    public async Task<IActionResult> SalaryHistoryReview([FromBody] ReviewHistoryRequestDto body)
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

        var result = await _loanService.SalaryHistoryReview(body);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
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
