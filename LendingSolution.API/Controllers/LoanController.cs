using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/loan")]
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
}