using LendingSolution.API.Auth;
using LendingSolution.Application.Exceptions;
using LendingSolution.Core.Auth;
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
    IFinanceService financeService,
    IRemitaService remitaService,
    ILogger<LoanController> logger
) : Controller
{
    private readonly ILoanService _loanService = loanService;
    private readonly ISalaryHistoryViewService _salaryHistoryViewService = salaryHistoryViewService;
    private readonly IFinanceService _financeService = financeService;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly ILogger<LoanController> _logger = logger;

    /// <summary>
    /// Gets all loans across the system
    /// </summary>
    /// <returns>A list of all loans with borrower and company details</returns>
    [HttpGet]
    [Route("/")]
    [HasPermission(Permissions.Loans.View)]
    public async Task<IActionResult> GetAllLoans()
    {
        try
        {
            var loans = await _loanService.GetAllLoans();
            var result = User.IsSuperAdmin() ? loans : loans.Where(l => User.CanAccessCompany(l.CompanyId)).ToList();
            _logger.LogInformation("Successfully fetched all loans");
            return Ok(ApiResponse.Ok("Loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all loans filtered by a specific status
    /// </summary>
    /// <param name="status">The loan status to filter by (e.g., Pending, Approved, Disbursed, Rejected)</param>
    /// <returns>A list of loans matching the specified status</returns>
    [HttpGet("status/{status}")]
    [HasPermission(Permissions.Loans.View)]
    public async Task<IActionResult> GetLoansByStatus(LoanStatus status)
    {
        try
        {
            var loans = await _loanService.GetLoansByStatus(status);
            var result = User.IsSuperAdmin() ? loans : loans.Where(l => User.CanAccessCompany(l.CompanyId)).ToList();
            _logger.LogInformation("Successfully fetched loans with status: {Status}", status);
            return Ok(ApiResponse.Ok($"Loans with status {status} fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loans by status.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Approves a pending loan application
    /// </summary>
    /// <param name="id">The unique identifier of the loan to approve</param>
    /// <param name="request">The approval details including optional reason</param>
    /// <returns>Updated loan details with approved status and approval timestamp</returns>
    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<IActionResult> ApproveLoan(Guid id, [FromBody] LoanApprovalRequestDto request)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, id))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.ApproveLoan(id, userId, request.Reason);
            
            _logger.LogInformation("Loan {LoanId} approved successfully by user {UserId}", id, userId);
            return Ok(ApiResponse.Ok("Loan approved successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while approving loan {LoanId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Rejects a pending loan application
    /// </summary>
    /// <param name="id">The unique identifier of the loan to reject</param>
    /// <param name="request">The rejection details including the reason for rejection</param>
    /// <returns>Updated loan details with rejected status and rejection reason</returns>
    [HttpPost("{id:guid}/reject")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<IActionResult> RejectLoan(Guid id, [FromBody] LoanRejectionRequestDto request)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, id))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.RejectLoan(id, userId, request.Reason);
            
            _logger.LogInformation("Loan {LoanId} rejected successfully by user {UserId}", id, userId);
            return Ok(ApiResponse.Ok("Loan rejected successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rejecting loan {LoanId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Processes a loan application with specified action (approve or reject)
    /// </summary>
    /// <param name="id">The unique identifier of the loan to process</param>
    /// <param name="request">The processing action and reason</param>
    /// <returns>Updated loan details with new status based on the action taken</returns>
    [HttpPost("{id:guid}/process")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<IActionResult> ProcessLoan(Guid id, [FromBody] ProcessLoanRequestDto request)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, id))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var result = await _loanService.ProcessLoan(id, request, userId);
            
            _logger.LogInformation("Loan {LoanId} processed successfully with action {Action} by user {UserId}", 
                id, request.Action, userId);
            return Ok(ApiResponse.Ok($"Loan {request.Action}d successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing loan {LoanId}.", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets salary history for all borrower applications in the admin's company
    /// </summary>
    /// <param name="filters">Filter and pagination parameters for salary history records</param>
    /// <returns>Paginated list of salary history records for the company's borrowers with detailed payment information</returns>
    [HttpGet("salary-history")]
    [HasPermission(Permissions.Loans.View)]
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
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company salary history");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets detailed salary history information for a specific record (Admin can only access their company's records)
    /// </summary>
    /// <param name="salaryHistoryId">The unique identifier of the salary history record</param>
    /// <returns>Detailed salary history information including payment dates, amounts, and employer details</returns>
    [HttpGet("salary-history/{salaryHistoryId}")]
    [HasPermission(Permissions.Loans.View)]
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
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all salary history records across all companies (SuperAdmin only)
    /// </summary>
    /// <param name="filters">Filter and pagination parameters for searching salary history records</param>
    /// <returns>Paginated list of all salary history records system-wide with borrower and company information</returns>
    [HttpGet("salary-history/all")]
    [Authorize(Roles = "SuperAdmin")]
    [Tags("SuperAdmin")]
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
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all salary history records");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets detailed salary history information for any record across all companies (SuperAdmin only)
    /// </summary>
    /// <param name="salaryHistoryId">The unique identifier of the salary history record</param>
    /// <returns>Comprehensive salary history details including all payment transactions and employment information</returns>
    [HttpGet("salary-history/details/{salaryHistoryId}")]
    [Authorize(Roles = "SuperAdmin")]
    [Tags("SuperAdmin")]
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
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    #region Offer Letter and Disbursement Endpoints
    
    /// <summary>
    /// Gets offer letter details and URL for a specific loan
    /// </summary>
    /// <param name="id">The unique identifier of the loan</param>
    /// <returns>Offer letter details including document URL, loan terms, repayment schedule, and status</returns>
    [HttpGet("{id:guid}/offer-letter")]
    [HasPermission(Permissions.Loans.View)]
    public async Task<IActionResult> GetOfferLetterDetails(Guid id)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, id))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var result = await _loanService.GetOfferLetterDetailsAsync(id);
            
            _logger.LogInformation("Successfully fetched offer letter details for loan {LoanId}", id);
            
            return Ok(ApiResponse.Ok("Offer letter details fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching offer letter details for loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    /// <summary>
    /// Sends or resends the offer letter email to the borrower for an approved loan
    /// </summary>
    /// <param name="id">The unique identifier of the approved loan</param>
    /// <returns>Confirmation with offer letter details and email delivery status</returns>
    [HttpPost("{id:guid}/send-offer-letter")]
    [HasPermission(Permissions.Loans.Manage)]
    public async Task<IActionResult> SendOfferLetter(Guid id)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, id))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.SendOfferLetterAsync(id, userId);
            
            _logger.LogInformation("Offer letter sent for loan {LoanId} by user {UserId}", id, userId);
            
            return Ok(ApiResponse.Ok("Offer letter sent successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending offer letter for loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    
    /// <summary>
    /// Disburses loan funds to the borrower's account after signed offer letter is received
    /// </summary>
    /// <param name="id">The unique identifier of the loan with signed offer letter</param>
    /// <returns>Disbursement confirmation with transaction reference, amount disbursed, and due date</returns>
    [HttpPost("{id:guid}/disburse")]
    [HasPermission(Permissions.Loans.Disburse)]
    public async Task<IActionResult> DisburseLoan(Guid id)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, id))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.DisburseLoanAsync(id, userId);
            
            _logger.LogInformation("Loan {LoanId} disbursed by user {UserId}", id, userId);
            
            return Ok(ApiResponse.Ok("Loan disbursed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while disbursing loan {LoanId}", id);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    /// <summary>
    /// Gets disbursements filtered by company or all disbursements based on user role
    /// </summary>
    /// <param name="companyId">Optional company ID to filter disbursements (SuperAdmin only). If not provided, SuperAdmin sees all disbursements, Admin sees only their company's disbursements.</param>
    /// <param name="page">Page number, starting at 1</param>
    /// <param name="pageSize">Items per page (1-100, default 20)</param>
    /// <returns>Page of disbursements, newest first, with paging metadata</returns>
    /// <response code="200">Returns the list of disbursements successfully</response>
    /// <response code="400">Bad request - Invalid company ID format or Admin user attempting to filter by companyId</response>
    /// <response code="401">Unauthorized - User not authenticated</response>
    /// <response code="403">Forbidden - User does not have required role (Admin or SuperAdmin)</response>
    /// <response code="500">Internal server error - An unexpected error occurred</response>
    [HttpGet("disbursements")]
    [HasPermission(Permissions.Loans.View)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PagedDisbursementListDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<object>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiResponse<object>))]
    public async Task<IActionResult> GetDisbursements(
        [FromQuery] Guid? companyId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperAdmin = userRoles.Contains("SuperAdmin");
            
            // If companyId is provided in query, only SuperAdmin can use it
            if (companyId.HasValue && !isSuperAdmin)
            {
                return StatusCode(403, ApiResponse.Fail("Only SuperAdmin can filter disbursements by company ID"));
            }
            
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);
            PagedDisbursementListDto result;
            
            if (isSuperAdmin)
            {
                if (companyId.HasValue)
                {
                    // SuperAdmin filtering by specific company
                    result = await _financeService.GetDisbursementsAsync(companyId.Value, page, pageSize);
                    _logger.LogInformation("SuperAdmin fetched disbursements for company {CompanyId}", companyId.Value);
                }
                else
                {
                    // SuperAdmin viewing all disbursements
                    result = await _financeService.GetDisbursementsAsync(null, page, pageSize);
                    _logger.LogInformation("SuperAdmin fetched all disbursements");
                }
            }
            else
            {
                // Admin can only see their own company's disbursements
                var companyIdClaim = User.FindFirstValue("CompanyId");
                if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var adminCompanyId))
                {
                    return BadRequest(ApiResponse.Fail("Company ID not found in token"));
                }
                
                result = await _financeService.GetDisbursementsAsync(adminCompanyId, page, pageSize);
                _logger.LogInformation("Admin fetched disbursements for their company {CompanyId}", adminCompanyId);
            }
            
            return Ok(ApiResponse.Ok("Disbursements fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching disbursements.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    /// <summary>
    /// Stops Remita mandate collection for a loan
    /// </summary>
    /// <param name="loanId">The unique identifier of the loan to stop collection for</param>
    /// <returns>Result of the stop mandate operation</returns>
    [HttpPost("{loanId:guid}/stop-collection")]
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<IActionResult> StopLoanCollection(Guid loanId)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, loanId))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.StopLoanCollectionAsync(loanId, userId);
            
            _logger.LogInformation("Loan collection stopped successfully for loan {LoanId} by user {UserId}", loanId, userId);
            return Ok(ApiResponse.Ok("Loan collection stopped successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping loan collection for {LoanId}.", loanId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Performs hybrid reconciliation of loan collection by comparing local records with Remita mandate history
    /// </summary>
    /// <param name="loanId">The unique identifier of the loan to reconcile</param>
    /// <returns>Reconciliation result with comparison details</returns>
    [HttpPost("{loanId:guid}/reconcile")]
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<IActionResult> ReconcileLoanCollection(Guid loanId)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, loanId))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _loanService.ReconcileLoanCollectionAsync(loanId, userId);
            
            _logger.LogInformation("Loan reconciliation completed for loan {LoanId} by user {UserId}. IsReconciled: {IsReconciled}", 
                loanId, userId, result.IsReconciled);
            
            return Ok(ApiResponse.Ok(result.Message, result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reconciling loan {LoanId}.", loanId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
    
    #endregion
}