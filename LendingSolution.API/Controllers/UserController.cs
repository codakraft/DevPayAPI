using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
public class UserController(
    IAuthService authService,
    ILoanService loanService,
    ILoanProductService loanProductService) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ILoanService _loanService = loanService;

    [HttpPost("onboarding")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = await _loanService.Register(body);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = await _authService.Login(body);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = _authService.VerifyOtp(body);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("save-personal-details")]
    public async Task<IActionResult> SavePersonalDetails([FromBody] SavePersonalDetailsRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = await _authService.SavePersonalDetails(body, User);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpPost("salary-history-review")]
    public async Task<IActionResult> SalaryHistoryReview([FromBody] ReviewHistoryRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = await _loanService.SalaryHistoryReview(body);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    [HttpGet("get-loan-products/{companyId}")]
    public async Task<IActionResult> GetLoanProductsByCompanyId([FromRoute] Guid companyId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = await _loanProductService.GetLoanProductsByCompanyId(companyId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("submit/{loanId}")]
    public async Task<IActionResult> SubmitLoan([FromRoute] Guid loanId, [FromBody] SubmitRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail("Invalid model state"));
        }

        var result = await _loanService.SubmitLoan(loanId, body);

        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }
}
