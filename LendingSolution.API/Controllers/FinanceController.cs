using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/finance")]
[Authorize(Roles = "FinanceOfficer,SuperAdmin")]
public class FinanceController(
    IFinanceService financeService,
    ILogger<FinanceController> logger) : Controller
{
    private readonly IFinanceService _financeService = financeService;
    private readonly ILogger<FinanceController> _logger = logger;

    // [GET]    /api/finance/disbursements
    [HttpGet("disbursements")]
    public async Task<IActionResult> GetAllDisbursements()
    {
        try
        {
            var result = await _financeService.GetAllDisbursementsAsync();
            _logger.LogInformation("Successfully fetched all disbursements");
            return Ok(ApiResponse.Ok("Disbursements fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching disbursements.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/repayments
    [HttpGet("repayments")]
    public async Task<IActionResult> GetAllRepayments()
    {
        try
        {
            var result = await _financeService.GetAllRepaymentsAsync();
            _logger.LogInformation("Successfully fetched all repayments");
            return Ok(ApiResponse.Ok("Repayments fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching repayments.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/finance/disbursements/{loanId}
    [HttpPost("disbursements/{loanId}")]
    public async Task<IActionResult> ProcessDisbursement(string loanId, [FromBody] DisbursementRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _financeService.ProcessDisbursementAsync(loanId, request, userId);
            _logger.LogInformation("Successfully processed disbursement for loan {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Disbursement processed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing disbursement.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/finance/repayments
    [HttpPost("repayments")]
    public async Task<IActionResult> ProcessRepayment([FromBody] RepaymentRequestDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _financeService.ProcessRepaymentAsync(request, userId);
            _logger.LogInformation("Successfully processed repayment");
            return Ok(ApiResponse.Ok("Repayment processed successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing repayment.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/reports/monthly
    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyFinanceReport([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            var result = await _financeService.GetMonthlyFinanceReportAsync(year, month);
            _logger.LogInformation("Successfully fetched monthly finance report for {Year}-{Month}", year, month);
            return Ok(ApiResponse.Ok("Monthly finance report fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching monthly finance report.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/reports/company/{companyId}
    [HttpGet("reports/company/{companyId}")]
    public async Task<IActionResult> GetCompanyFinanceReport(string companyId)
    {
        try
        {
            var result = await _financeService.GetCompanyFinanceReportAsync(companyId);
            _logger.LogInformation("Successfully fetched finance report for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company finance report fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company finance report.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/wallet/{companyId}
    [HttpGet("wallet/{companyId}")]
    public async Task<IActionResult> GetCompanyWallet(string companyId)
    {
        var result = await _financeService.GetCompanyWalletAsync(companyId);
        _logger.LogInformation("Successfully fetched wallet for company {CompanyId}", companyId);
        return Ok(ApiResponse.Ok("Company wallet fetched successfully", result));
    }
}

