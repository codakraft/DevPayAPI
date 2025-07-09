using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos.Response.Remita;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/remita")]
public class RemitaController(
    IRemitaService remitaService,
    ILogger<RemitaController> logger) : Controller
{
    private readonly IRemitaService _remitaService = remitaService;
    private readonly ILogger<RemitaController> _logger = logger;

    /// <summary>
    /// Step 1: Get borrower's salary history from Remita
    /// </summary>
    /// <param name="request">BVN and other borrower details</param>
    /// <returns>Salary history and employment information</returns>
    [HttpPost("salary-history")]
    [AllowAnonymous] // Public endpoint for loan applications
    public async Task<IActionResult> GetSalaryHistory([FromBody] SalaryHistoryRequestDto request)
    {
        try
        {
            var result = await _remitaService.GetSalaryHistoryByBvnAsync(request);
            _logger.LogInformation("Salary history retrieved successfully for BVN: {BVN}", request.Bvn);
            return Ok(ApiResponse.Ok("Salary history retrieved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching salary history.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 2: Verify customer account details with bank
    /// </summary>
    /// <param name="request">Account number and bank code</param>
    /// <returns>Account verification status and details</returns>
    [HttpPost("verify-account")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyAccount([FromBody] AccountVerificationRequestDto request)
    {
        try
        {
            var result = await _remitaService.VerifyAccountAsync(request);
            _logger.LogInformation("Account verified successfully: {AccountNumber}", request.AccountNumber);
            return Ok(ApiResponse.Ok("Account verified successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while verifying account.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 3: Create loan application and mandate setup
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <param name="request">Loan details and borrower information</param>
    /// <returns>Mandate reference and OTP request details</returns>
    [HttpPost("loans/{loanId}/mandate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateLoanMandate(Guid loanId, [FromBody] CreateMandateRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _remitaService.CreateLoanMandateAsync(loanId, request, userId);
            _logger.LogInformation("Loan mandate created successfully for loan: {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Loan mandate created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating loan mandate.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 4: Validate mandate with OTP from borrower
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <param name="request">OTP code from borrower</param>
    /// <returns>Mandate activation status</returns>
    [HttpPost("loans/{loanId}/mandate/validate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ValidateMandate(Guid loanId, [FromBody] ValidateMandateOtpRequestDto request)
    {
        try
        {
            var result = await _remitaService.ValidateMandate(loanId, request);
            _logger.LogInformation("Mandate validated successfully for loan: {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Mandate validated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating mandate.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 5: Process loan disbursement via Remita
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <param name="request">Disbursement details</param>
    /// <returns>Disbursement transaction reference</returns>
    [HttpPost("loans/{loanId}/disburse")]
    [Authorize(Roles = "FinanceOfficer,SuperAdmin")]
    public async Task<IActionResult> DisburseLoan(Guid loanId, [FromBody] RemitaDisbursementRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _remitaService.ProcessLoanDisbursementAsync(loanId, request, userId);
            _logger.LogInformation("Loan disbursed successfully: {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Loan disbursed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disbursing loan.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 6: Collect repayment via standing order mandate
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <param name="request">Repayment collection request</param>
    /// <returns>Collection transaction status</returns>
    [HttpPost("loans/{loanId}/collect-repayment")]
    [Authorize(Roles = "FinanceOfficer,SuperAdmin")]
    public async Task<IActionResult> CollectRepayment(Guid loanId, [FromBody] RepaymentCollectionRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _remitaService.CollectRepaymentAsync(loanId, request, userId);
            _logger.LogInformation("Repayment collected successfully for loan: {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Repayment collected successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while collecting repayment.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Step 7: Check status of any Remita transaction
    /// </summary>
    /// <param name="transactionRef">Remita transaction reference</param>
    /// <returns>Transaction status and details</returns>
    [HttpGet("transactions/{transactionRef}/status")]
    [Authorize(Roles = "Admin,FinanceOfficer,SuperAdmin")]
    public async Task<IActionResult> GetTransactionStatus(string transactionRef)
    {
        try
        {
            var result = await _remitaService.GetTransactionStatusAsync(transactionRef);
            _logger.LogInformation("Transaction status retrieved successfully: {TransactionRef}", transactionRef);
            return Ok(ApiResponse.Ok("Transaction status retrieved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting transaction status.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get list of supported banks from Remita
    /// </summary>
    /// <returns>List of banks with codes</returns>
    [HttpGet("banks")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBanks()
    {
        try
        {
            var result = await _remitaService.GetBanksAsync();
            _logger.LogInformation("Banks retrieved successfully");
            return Ok(ApiResponse.Ok("Banks retrieved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching banks.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Webhook endpoint for Remita notifications
    /// </summary>
    /// <param name="notification">Webhook notification from Remita</param>
    /// <returns>Acknowledgment response</returns>
    [HttpPost("webhook/notifications")]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhookNotification([FromBody] RemitaWebhookNotificationDto notification)
    {
        _logger.LogInformation("Received Remita webhook notification: {NotificationType}", notification.NotificationType);
        
        var result = await _remitaService.ProcessWebhookNotificationAsync(notification);
        _logger.LogInformation("Remita webhook notification processed successfully");
        
        return Ok(new { status = "received", processed = result });
    }
}
