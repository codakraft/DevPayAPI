using LendingSolution.API.Auth;
using LendingSolution.Application.Exceptions;
using LendingSolution.Core.Auth;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using LendingSolution.Core.Models;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/finance")]
[Authorize]
public class FinanceController(
    IFinanceService financeService,
    ILoanService loanService,
    UserManager<ApplicationUser> userManager,
    ILogger<FinanceController> logger) : Controller
{
    private readonly IFinanceService _financeService = financeService;
    private readonly ILoanService _loanService = loanService;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<FinanceController> _logger = logger;

    // [GET]    /api/finance/repayments
    [HttpGet("repayments")]
    [HasPermission(Permissions.Finance.View)]
    public async Task<IActionResult> GetAllRepayments([FromQuery] RepaymentFilterDto filters)
    {
        try
        {
            if (!User.IsSuperAdmin())
            {
                // Non-SuperAdmins only ever see their own company's repayments
                filters.CompanyId = User.GetCompanyId()
                    ?? throw new AppException("Company ID not found in token", 400);
            }

            var result = await _financeService.GetAllRepaymentsAsync(filters);
            _logger.LogInformation("Successfully fetched repayments with filters");
            return Ok(ApiResponse.Ok("Repayments fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching repayments.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/finance/disbursements/{loanId}
    [HttpPost("disbursements/{loanId}")]
    [HasPermission(Permissions.Loans.Disburse)]
    public async Task<IActionResult> ProcessDisbursement(string loanId, [FromBody] DisbursementRequestDto request)
    {
        try
        {
            if (!Guid.TryParse(loanId, out var loanGuid) || !await User.CanAccessLoanAsync(_loanService, loanGuid))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _financeService.ProcessDisbursementAsync(loanId, request, userId);
            _logger.LogInformation("Successfully processed disbursement for loan {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Disbursement processed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing disbursement.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/finance/repayments
    [HttpPost("repayments")]
    [HasPermission(Permissions.Finance.Manage)]
    public async Task<IActionResult> ProcessRepayment([FromBody] RepaymentRequestDto request)
    {
        try
        {
            if (!Guid.TryParse(request.LoanId, out var loanGuid) || !await User.CanAccessLoanAsync(_loanService, loanGuid))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _financeService.ProcessRepaymentAsync(request, userId);
            _logger.LogInformation("Successfully processed repayment");
            return Ok(ApiResponse.Ok("Repayment processed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing repayment.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/reports/monthly
    [HttpGet("reports/monthly")]
    [HasPermission(Permissions.Finance.View)]
    public async Task<IActionResult> GetMonthlyFinanceReport(
        [FromQuery] int year, 
        [FromQuery] int month,
        [FromQuery] Guid? companyId = null)
    {
        try
        {
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperAdmin = userRoles.Contains("SuperAdmin");

            // If SuperAdmin, companyId is required
            if (isSuperAdmin && !companyId.HasValue)
            {
                return BadRequest(ApiResponse.Fail("CompanyId is required for SuperAdmin users"));
            }

            // If not SuperAdmin, get companyId from user's company
            if (!isSuperAdmin)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null || string.IsNullOrEmpty(user.CompanyId))
                {
                    return BadRequest(ApiResponse.Fail("User company not found"));
                }
                companyId = Guid.Parse(user.CompanyId);
            }

            var result = await _financeService.GetMonthlyFinanceReportAsync(year, month, companyId);
            _logger.LogInformation("Successfully fetched monthly finance report for {Year}-{Month}", year, month);
            return Ok(ApiResponse.Ok("Monthly finance report fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching monthly finance report.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/reports/company/{companyId}
    [HttpGet("reports/company/{companyId}")]
    [HasPermission(Permissions.Finance.View)]
    public async Task<IActionResult> GetCompanyFinanceReport(string companyId)
    {
        try
        {
            if (!Guid.TryParse(companyId, out var companyGuid) || !User.CanAccessCompany(companyGuid))
            {
                return StatusCode(403, ApiResponse.Fail("You can only access your own company's data"));
            }

            var result = await _financeService.GetCompanyFinanceReportAsync(companyId);
            _logger.LogInformation("Successfully fetched finance report for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company finance report fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company finance report.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/wallet/{companyId}
    [HttpGet("wallet/{companyId}")]
    [HasPermission(Permissions.Finance.View)]
    public async Task<IActionResult> GetCompanyWallet(string companyId)
    {
        if (!Guid.TryParse(companyId, out var companyGuid) || !User.CanAccessCompany(companyGuid))
        {
            return StatusCode(403, ApiResponse.Fail("You can only access your own company's data"));
        }

        var result = await _financeService.GetCompanyWalletAsync(companyId);
        _logger.LogInformation("Successfully fetched wallet for company {CompanyId}", companyId);
        return Ok(ApiResponse.Ok("Company wallet fetched successfully", result));
    }
}

