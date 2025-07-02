using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;
using LendingSolution.Application.Exceptions;
using Microsoft.OpenApi.Exceptions;

namespace LendingSolution.API.Controllers;

[ApiController]
public class BorrowerController(
    IAuthService authService,
    ILoanService loanService,
    ILoanProductService loanProductService,
    IRemitaService remitaService,
    ILogger<BorrowerController> logger
) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ILoanService _loanService = loanService;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly ILogger<BorrowerController> _logger = logger;

    [HttpPost("onboarding")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto body)
    {
        try
        {
            var result = await _loanService.Register(body);
            return Ok(ApiResponse.Ok("User registered successfully", new { loanId = result }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during user onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during registration");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequestDto body)
    {
        try
        {
            var result = _authService.VerifyOtp(body);
            return Ok(ApiResponse.Ok("OTP verification successful", new { }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error verifying OTP");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during OTP verification");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    [HttpPost("save-personal-details")]
    public async Task<IActionResult> SavePersonalDetails([FromBody] SavePersonalDetailsRequestDto body)
    {
        try
        {

            var result = await _authService.SavePersonalDetails(body, User);
            return Ok(ApiResponse.Ok("Personal details saved successfully"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error saving personal details");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error saving personal details");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    [HttpPost("salary-history-review/{loanID}")]
    public async Task<IActionResult> SalaryHistoryReview([FromRoute] Guid loanId, [FromBody] ReviewHistoryRequestDto body)
    {
        try
        {
            var result = await _remitaService.GetSalaryHistory(loanId, body);
            return Ok(ApiResponse.Ok("Salary history review successful"));

        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Error");
            return StatusCode(400, ApiResponse.Fail("Something went wrong"));
        }
    }

    [HttpGet("loan-products/{companyId}")]
    public async Task<IActionResult> GetLoanProductsByCompanyId([FromRoute] Guid companyId)
    {
        try
        {
            var result = await _loanProductService.GetLoanProductsByCompanyId(companyId);
            _logger.LogError("Loan products retrieved successfully");
            return Ok(ApiResponse.Ok("Loan products retrieved successfully", result));
        }
        catch (OpenApiException ex)
        {
            _logger.LogError(ex, "Error fetching loan products");
            return StatusCode(404, ApiResponse.Fail("Company not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error fetching loan products");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    [HttpPost("submit/{loanId}")]
    public async Task<IActionResult> SetMandate([FromRoute] Guid loanId, [FromBody] SubmitRequestDto body)
    {
        try
        {

            var result = await _remitaService.GenerateMandate(loanId, body);
            _logger.LogInformation("Mandate generated successfully for loan {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Enter otp to continue"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error submitting loan");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during loan submission");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    [HttpPost("validate/mandate/{loanId}")]
    public async Task<IActionResult> ValidateMandate([FromRoute] Guid loanId, ValidateMandateOtpRequestDto body)
    {
        try
        {
            var result = await _remitaService.ValidateMandate(loanId, body);
            return Ok(result);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Unhandled error");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            return StatusCode(500, ApiResponse.Fail("Somethign went wrong."));

        }

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

    // [GET]    /api/loans/user/{userId}       // View all loan applications by the user  
    // [GET]    /api/loans/{id}                // Get details of a specific loan  
    // [GET]    /api/loans/status/{status}     // (Optional) Filter user loans by status  
    // [GET]    /api/finance/disbursements     // User or admin can check which loans were disbursed  
}

