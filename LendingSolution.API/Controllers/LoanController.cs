using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/loan")]
public class LoanController(
    ILoanService loanService,
    ISalaryHistoryViewService salaryHistoryViewService,
    ILogger<LoanController> logger
) : Controller
{
    private readonly ILoanService _loanService = loanService;
    private readonly ISalaryHistoryViewService _salaryHistoryViewService = salaryHistoryViewService;
    private readonly ILogger<LoanController> _logger = logger;

    [HttpGet]
    [Route("/")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllLoans()
    {
        try
        {
            var result = await _loanService.GetAllLoans();
            _logger.LogInformation("Successfully fetched all loans");
            return Ok(ApiResponse.Ok("Loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("status/{status}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetLoansByStatus(LoanStatus status)
    {
        try
        {
            var result = await _loanService.GetLoansByStatus(status);
            _logger.LogInformation("Successfully fetched loans with status: {Status}", status);
            return Ok(ApiResponse.Ok($"Loans with status {status} fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loans by status.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ApproveLoan(Guid id, [FromBody] LoanApprovalRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.ApproveLoan(id, userId, request.Reason);
            
            _logger.LogInformation("Loan {LoanId} approved successfully by user {UserId}", id, userId);
            return Ok(ApiResponse.Ok("Loan approved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving loan {LoanId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RejectLoan(Guid id, [FromBody] LoanRejectionRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.RejectLoan(id, userId, request.Reason);
            
            _logger.LogInformation("Loan {LoanId} rejected successfully by user {UserId}", id, userId);
            return Ok(ApiResponse.Ok("Loan rejected successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting loan {LoanId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("{id:guid}/process")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ProcessLoan(Guid id, [FromBody] ProcessLoanRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _loanService.ProcessLoan(id, request, userId);
            
            _logger.LogInformation("Loan {LoanId} processed successfully with action {Action} by user {UserId}", 
                id, request.Action, userId);
            return Ok(ApiResponse.Ok($"Loan {request.Action}d successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing loan {LoanId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get salary history for borrower applications in the company (Admin access)
    /// </summary>
    [HttpGet("salary-history")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCompanySalaryHistory([FromQuery] SalaryHistoryFilterRequestDto filters)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company information not found"));
            }

            var result = await _salaryHistoryViewService.GetCompanySalaryHistoryAsync(companyId, filters);
            
            _logger.LogInformation("Successfully fetched {Count} salary history records for company {CompanyId}", 
                result.Data.Count, companyId);
            
            return Ok(ApiResponse.Ok("Salary history fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company salary history");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get detailed salary history by ID (Admin access - only for their company's records)
    /// </summary>
    [HttpGet("salary-history/{salaryHistoryId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetSalaryHistoryDetails(Guid salaryHistoryId)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company information not found"));
            }

            // Validate company access to this salary history record
            var hasAccess = await _salaryHistoryViewService.ValidateCompanyAccessAsync(companyId, salaryHistoryId);
            if (!hasAccess)
            {
                return Forbid("You don't have access to this salary history record");
            }

            var result = await _salaryHistoryViewService.GetSalaryHistoryDetailsAsync(salaryHistoryId);
            
            _logger.LogInformation("Successfully fetched salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            
            return Ok(ApiResponse.Ok("Salary history details fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get all salary history records across all companies (SuperAdmin access)
    /// </summary>
    [HttpGet("salary-history/all")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllSalaryHistory([FromQuery] SalaryHistoryFilterRequestDto filters)
    {
        try
        {
            var result = await _salaryHistoryViewService.GetAllSalaryHistoryAsync(filters);
            
            _logger.LogInformation("Successfully fetched {Count} salary history records across all companies", 
                result.Data.Count);
            
            return Ok(ApiResponse.Ok("All salary history fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all salary history records");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Get detailed salary history by ID (SuperAdmin access - can access any record)
    /// </summary>
    [HttpGet("salary-history/details/{salaryHistoryId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAnySalaryHistoryDetails(Guid salaryHistoryId)
    {
        try
        {
            var result = await _salaryHistoryViewService.GetSalaryHistoryDetailsAsync(salaryHistoryId);
            
            _logger.LogInformation("SuperAdmin successfully fetched salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            
            return Ok(ApiResponse.Ok("Salary history details fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    #region Offer Letter and Disbursement Endpoints
    
    /// <summary>
    /// Get offer letter details for a loan
    /// </summary>
    [HttpGet("{id:guid}/offer-letter")]
    [Authorize]
    public async Task<IActionResult> GetOfferLetterDetails(Guid id)
    {
        try
        {
            var result = await _loanService.GetOfferLetterDetailsAsync(id);
            
            _logger.LogInformation("Successfully fetched offer letter details for loan {LoanId}", id);
            
            return Ok(ApiResponse.Ok("Offer letter details fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching offer letter details for loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    /// <summary>
    /// Send or resend offer letter for an approved loan
    /// </summary>
    [HttpPost("{id:guid}/send-offer-letter")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> SendOfferLetter(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.SendOfferLetterAsync(id, userId);
            
            _logger.LogInformation("Offer letter sent for loan {LoanId} by user {UserId}", id, userId);
            
            return Ok(ApiResponse.Ok("Offer letter sent successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending offer letter for loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    /// <summary>
    /// Upload signed offer letter (PDF only)
    /// </summary>
    [HttpPost("{id:guid}/upload-signed-offer-letter")]
    [Authorize]
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
    
    /// <summary>
    /// Disburse loan after signed offer letter is received
    /// </summary>
    [HttpPost("{id:guid}/disburse")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DisburseLoan(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.DisburseLoanAsync(id, userId);
            
            _logger.LogInformation("Loan {LoanId} disbursed by user {UserId}", id, userId);
            
            return Ok(ApiResponse.Ok("Loan disbursed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disbursing loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    #endregion
}