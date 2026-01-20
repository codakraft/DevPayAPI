using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using LendingSolution.Application.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BorrowerController(
    IBorrowerOnboardingService borrowerOnboardingService,
    IBorrowerApplicationRepository borrowerApplicationRepository,
    IRemitaService remitaService,
    ILoanService loanService,
    ILogger<BorrowerController> logger
) : Controller
{
    private readonly IBorrowerOnboardingService _borrowerOnboardingService = borrowerOnboardingService;
    private readonly IBorrowerApplicationRepository _borrowerApplicationRepository = borrowerApplicationRepository;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly ILoanService _loanService = loanService;
    private readonly ILogger<BorrowerController> _logger = logger;

    /// <summary>
    /// Step 1: Initial borrower registration with basic information and company selection
    /// </summary>
    /// <param name="request">Borrower's basic details including name, email, phone, and company ID</param>
    /// <returns>Loan ID for the created application and confirmation message</returns>
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

    /// <summary>
    /// Step 1B: Validates email OTP sent during Step 1
    /// </summary>
    /// <param name="request">Loan ID and OTP code for verification</param>
    /// <returns>Confirmation of email verification and loan ID</returns>
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

    /// <summary>
    /// Step 2: Submits BVN for verification and sends OTP to borrower's phone
    /// </summary>
    /// <param name="request">BVN and loan ID</param>
    /// <returns>Confirmation that BVN OTP has been sent</returns>
    [HttpPost("step2")]
    public async Task<IActionResult> Step2([FromBody] BorrowerStep2RequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step2_SaveBvnAsync(request);
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

    /// <summary>
    /// Step 2B: Validates BVN OTP sent during Step 2
    /// </summary>
    /// <param name="request">Loan ID and OTP code for BVN verification</param>
    /// <returns>Confirmation of BVN verification and loan ID</returns>
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

    /// <summary>
    /// Step 3: Collects bank account details, address and uploads identity documents (ID card, selfie, utility bill)
    /// </summary>
    /// <param name="request">Bank account number, bank code, address details, document URLs/IDs, and loan ID</param>
    /// <returns>Loan eligibility details including min/max loan amount and tenor ranges</returns>
    [HttpPost("step3")]
    public async Task<IActionResult> Step3([FromBody] BorrowerStep3RequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.Step3_SaveBankAddressDocumentsAsync(request);
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

    /// <summary>
    /// Step 4: Final loan submission with selected amount and tenor
    /// </summary>
    /// <param name="request">Requested loan amount, tenor (duration in months), and loan ID</param>
    /// <returns>Repayment details including total repayment amount and monthly installment</returns>
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

    #region OTP Management Endpoints

    /// <summary>
    /// Resends email OTP for Step 1 verification when the original OTP expires or is not received
    /// </summary>
    /// <param name="request">Loan ID for which to resend the OTP</param>
    /// <returns>Confirmation that a new OTP has been sent to the registered email</returns>
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

    /// <summary>
    /// Resends BVN OTP for Step 2 verification when the original OTP expires or is not received
    /// </summary>
    /// <param name="request">Loan ID for which to resend the BVN OTP</param>
    /// <returns>Confirmation that a new BVN OTP has been sent to the registered phone</returns>
    [HttpPost("resend-step2-bvn-otp")]
    public async Task<IActionResult> ResendStep2BvnOtp([FromBody] ResendStep2BvnOtpRequestDto request)
    {
        try
        {
            var result = await _borrowerOnboardingService.ResendStep2BvnOtpAsync(request);
            return Ok(ApiResponse.Ok("BVN OTP has been resent successfully"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error resending Step 2 BVN OTP for loan ID: {LoanId}", request.LoanId);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error resending Step 2 BVN OTP for loan ID: {LoanId}", request.LoanId);
            return StatusCode(500, ApiResponse.Fail("Something went wrong"));
        }
    }

    /// <summary>
    /// Generates a new email OTP for verification purposes
    /// </summary>
    /// <param name="request">Email address to send the OTP to</param>
    /// <returns>Confirmation that OTP has been generated and sent</returns>
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

    /// <summary>
    /// Validates an email OTP code entered by the user
    /// </summary>
    /// <param name="request">Email address and OTP code to validate</param>
    /// <returns>Confirmation of successful OTP verification</returns>
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

    #endregion

    #region Application Status Endpoints

    /// <summary>
    /// Gets the current onboarding step status for a borrower application
    /// </summary>
    /// <param name="request">Email address or application ID to check status for</param>
    /// <returns>Current step number, completion status, and next required actions</returns>
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
    /// Gets the current onboarding step status by email or application ID (RESTful GET method)
    /// </summary>
    /// <param name="emailOrId">The borrower's email address or unique application ID</param>
    /// <returns>Detailed step information including progress percentage, loan eligibility, and next actions</returns>
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

    #endregion

    #region Document Management Endpoints

    /// <summary>
    /// Updates or re-uploads identity documents for a loan application
    /// </summary>
    /// <param name="request">Loan ID and updated document URLs/IDs (ID card, selfie, utility bill)</param>
    /// <returns>Confirmation that documents have been updated successfully</returns>
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

    #endregion

    #region Remita Integration Endpoints

    ///// <summary>
    ///// Get borrower salary history from Remita
    ///// </summary>
    //[HttpPost("remita/salary-history")]
    //public async Task<IActionResult> GetSalaryHistory([FromBody] RemitaSalaryHistoryRequestDto request)
    //{
    //    try
    //    {
    //        var result = await _remitaService.GetSalaryHistoryAsync(
    //            request.AccountNumber,
    //            request.BankCode,
    //            request.Bvn,
    //            request.BorrowerApplicationId,
    //            request.FirstName,
    //            request.LastName,
    //            request.MiddleName,
    //            string.IsNullOrWhiteSpace(request.AuthorisationCode) ? null : request.AuthorisationCode
    //        );

    //        if (result == null)
    //        {
    //            return BadRequest(ApiResponse.Fail("Failed to retrieve salary history"));
    //        }

    //        return Ok(ApiResponse.Ok("Salary history retrieved successfully", result.Data!));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving salary history for account {AccountNumber}", request.AccountNumber);
    //        return StatusCode(500, ApiResponse.Fail("Something went wrong"));
    //    }
    //}

    ///// <summary>
    ///// Create a loan mandate in Remita
    ///// </summary>
    //[HttpPost("remita/create-mandate")]
    //public async Task<IActionResult> CreateMandate([FromBody] RemitaCreateMandateRequestDto request)
    //{
    //    try
    //    {
    //        var result = await _remitaService.CreateMandateAsync(
    //            request.CustomerId,
    //            request.PhoneNumber,
    //            request.AccountNumber,
    //            request.LoanAmount,
    //            request.CollectionAmount,
    //            request.DateOfDisbursement,
    //            request.DateOfCollection,
    //            request.TotalCollectionAmount,
    //            request.NumberOfRepayments,
    //            request.BankCode,
    //            string.IsNullOrWhiteSpace(request.AuthorisationCode) ? null : request.AuthorisationCode
    //        );

    //        if (result == null)
    //        {
    //            return BadRequest(ApiResponse.Fail("Failed to create mandate"));
    //        }

    //        return Ok(ApiResponse.Ok("Mandate created successfully", result.Data!));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error creating mandate for customer {CustomerId}", request.CustomerId);
    //        return StatusCode(500, ApiResponse.Fail("Something went wrong"));
    //    }
    //}

    ///// <summary>
    ///// Stop an existing loan mandate in Remita
    ///// </summary>
    //[HttpPost("remita/stop-mandate")]
    //public async Task<IActionResult> StopMandate([FromBody] RemitaStopMandateRequestDto request)
    //{
    //    try
    //    {
    //        var result = await _remitaService.StopMandateAsync(
    //            request.CustomerId,
    //            request.MandateReference,
    //            request.AuthorisationCode
    //        );

    //        if (result == null)
    //        {
    //            return BadRequest(ApiResponse.Fail("Failed to stop mandate"));
    //        }

    //        return Ok(ApiResponse.Ok("Mandate stopped successfully", result.Data!));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error stopping mandate {MandateReference} for customer {CustomerId}",
    //            request.MandateReference, request.CustomerId);
    //        return StatusCode(500, ApiResponse.Fail("Something went wrong"));
    //    }
    //}

    ///// <summary>
    ///// Get mandate payment history from Remita
    ///// </summary>
    //[HttpPost("remita/mandate-history")]
    //public async Task<IActionResult> GetMandateHistory([FromBody] RemitaMandateHistoryRequestDto request)
    //{
    //    try
    //    {
    //        var result = await _remitaService.GetMandateHistoryAsync(
    //            request.CustomerId,
    //            request.MandateReference,
    //            request.AuthorisationCode
    //        );

    //        if (result == null)
    //        {
    //            return BadRequest(ApiResponse.Fail("Failed to retrieve mandate history"));
    //        }

    //        return Ok(ApiResponse.Ok("Mandate history retrieved successfully", result.Data!));
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving mandate history for customer {CustomerId}", request.CustomerId);
    //        return StatusCode(500, ApiResponse.Fail("Something went wrong"));
    //    }
    //}

    /// <summary>
    /// Uploads the signed offer letter document for a loan (PDF format only)
    /// </summary>
    /// <param name="id">The unique loan ID</param>
    /// <param name="dto">Document ID or URL of the signed offer letter PDF</param>
    /// <returns>Confirmation of upload with updated loan status</returns>
    [HttpPost("{id:guid}/upload-signed-offer-letter")]
    // [Authorize]
    public async Task<IActionResult> UploadSignedOfferLetter(Guid id, [FromBody] SignedOfferLetterUploadDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var result = await _loanService.UploadSignedOfferLetterAsync(id, dto, userId);
            
            _logger.LogInformation("Signed offer letter uploaded for loan {LoanId} by user {UserId}", id, userId);
            
            return Ok(ApiResponse.Ok("Signed offer letter uploaded successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while uploading signed offer letter for loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    #endregion
}