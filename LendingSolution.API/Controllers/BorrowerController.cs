using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BorrowerController(
    IBorrowerOnboardingService borrowerOnboardingService,
    IBorrowerApplicationRepository borrowerApplicationRepository,
    ILogger<BorrowerController> logger
) : Controller
{
    private readonly IBorrowerOnboardingService _borrowerOnboardingService = borrowerOnboardingService;
    private readonly IBorrowerApplicationRepository _borrowerApplicationRepository = borrowerApplicationRepository;
    private readonly ILogger<BorrowerController> _logger = logger;

    // Step 1: Initial borrower registration with basic info
    [HttpPost("step1")]
    public async Task<IActionResult> Step1([FromBody] BorrowerStep1RequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step1_SaveBorrowerInfoAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new { loanId = result.LoanId }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during Step 1 for borrower onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during Step 1");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Step 1B: Email OTP validation
    [HttpPost("step1b")]
    public async Task<IActionResult> Step1B([FromBody] BorrowerStep1BRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step1B_ValidateEmailOtpAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new { loanId = request.LoanId }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during Step 1B for borrower onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during Step 1B");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Step 2: Bank and BVN information
    [HttpPost("step2")]
    public async Task<IActionResult> Step2([FromBody] BorrowerStep2RequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step2_SaveBankBvnInfoAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new { loanId = request.LoanId }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during Step 2 for borrower onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during Step 2");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Step 2B: BVN OTP validation
    [HttpPost("step2b")]
    public async Task<IActionResult> Step2B([FromBody] BorrowerStep2BRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step2B_ValidateBvnOtpAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new { loanId = request.LoanId }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during Step 2B for borrower onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during Step 2B");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Step 3: Address and document upload
    [HttpPost("step3")]
    public async Task<IActionResult> Step3([FromBody] BorrowerStep3RequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step3_SaveAddressDocumentsAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new
            {
                loanId = request.LoanId,
                maxLoanEligible = result.MaxLoanEligible,
                minLoanEligible = result.MinLoanEligible,
                maxTenor = result.MaxTenor,
                minTenor = result.MinTenor
            }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during Step 3 for borrower onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during Step 3");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Step 4: Final loan submission
    [HttpPost("step4")]
    public async Task<IActionResult> Step4([FromBody] BorrowerStep4RequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step4_SubmitLoanApplicationAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new
            {
                loanId = request.LoanId,
                repaymentAmount = result.RepaymentAmount,
                tenor = result.Tenor,
                monthlyRepaymentAmount = result.MonthlyRepaymentAmount
            }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error during Step 4 for borrower onboarding");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error during Step 4");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Standalone OTP endpoints

    // Resend email OTP for Step 1 (when original expires)
    [HttpPost("resend-step1-email-otp")]
    public async Task<IActionResult> ResendStep1EmailOtp([FromBody] ResendStep1EmailOtpRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.ResendStep1EmailOtpAsync(request);
            return Ok(ApiResponse.Ok("Email OTP has been resent successfully"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error resending Step 1 email OTP for loan ID: {LoanId}", request.LoanId);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error resending Step 1 email OTP for loan ID: {LoanId}", request.LoanId);
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Generate email OTP
    [HttpPost("generate-email-otp")]
    public async Task<IActionResult> GenerateEmailOtp([FromBody] GenerateEmailOtpRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.GenerateEmailOtpAsync(request);
            return Ok(ApiResponse.Ok(result.Message));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error generating email OTP");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error generating email OTP");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Validate email OTP
    [HttpPost("validate-email-otp")]
    public async Task<IActionResult> ValidateEmailOtp([FromBody] ValidateEmailOtpRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.ValidateEmailOtpAsync(request);
            return Ok(ApiResponse.Ok(result.Message));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error validating email OTP");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error validating email OTP");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Generate BVN OTP
    [HttpPost("generate-bvn-otp")]
    public async Task<IActionResult> GenerateBvnOtp([FromBody] GenerateBvnOtpRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.GenerateBvnOtpAsync(request);
            return Ok(ApiResponse.Ok(result.Message));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error generating BVN OTP");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error generating BVN OTP");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Validate BVN OTP
    [HttpPost("validate-bvn-otp")]
    public async Task<IActionResult> ValidateBvnOtp([FromBody] ValidateBvnOtpRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.ValidateBvnOtpAsync(request);
            return Ok(ApiResponse.Ok(result.Message));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error validating BVN OTP");
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error validating BVN OTP");
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    // Get current step status for borrower application
    [HttpPost("current-step")]
    public async Task<IActionResult> GetCurrentStep([FromBody] BorrowerCurrentStepRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.GetCurrentStepAsync(request);
            return Ok(ApiResponse.Ok("Current step retrieved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error retrieving current step for email {Email}", request.Email);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error retrieving current step for email {Email}", request.Email);
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    /// <summary>
    /// Get current step status for borrower application (RESTful GET endpoint)
    /// </summary>
    /// <param name="emailOrId">The borrower's email address or application ID</param>
    /// <returns>Current step information including progress, eligibility, and required actions</returns>
    [HttpGet("{emailOrId}")]
    public async Task<IActionResult> GetCurrentStepByEmail(string emailOrId)
    {
        try
        {
            var request = new BorrowerCurrentStepRequestDto { Email = emailOrId };
            var result = await _borrowerOnboardingService.GetCurrentStepAsync(request);
            
            _logger.LogInformation("Current step retrieved successfully for {EmailOrId}. Current step: {CurrentStep}", 
                emailOrId, result.CurrentStep);
            
            return Ok(ApiResponse.Ok("Current step retrieved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error retrieving current step for {EmailOrId}", emailOrId);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error retrieving current step for {EmailOrId}", emailOrId);
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    /// <summary>
    /// Update/Re-add document images for loan application
    /// Can be used regardless of loan completion status
    /// </summary>
    [HttpPut("update-documents")]
    public async Task<IActionResult> UpdateDocuments([FromBody] UpdateDocumentsRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.UpdateDocumentsAsync(request);
            return Ok(ApiResponse.Ok(result.Message, new { loanId = request.LoanId }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error updating documents for loan {LoanId}", request.LoanId);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error updating documents for loan {LoanId}", request.LoanId);
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }
}