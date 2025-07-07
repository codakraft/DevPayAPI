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
            return Ok(result);
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
            return Ok(result);
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _financeService.ProcessDisbursementAsync(loanId, request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while processing disbursement for loan {LoanId}.", loanId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [POST]   /api/finance/repayments
    [HttpPost("repayments")]
    public async Task<IActionResult> ProcessRepayment([FromBody] RepaymentRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _financeService.ProcessRepaymentAsync(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
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
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching monthly finance report for {Year}-{Month}.", year, month);
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
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching finance report for company {CompanyId}.", companyId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // [GET]    /api/finance/wallet/{companyId}
    [HttpGet("wallet/{companyId}")]
    public async Task<IActionResult> GetCompanyWallet(string companyId)
    {
        try
        {
            var result = await _financeService.GetCompanyWalletAsync(companyId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching wallet for company {CompanyId}.", companyId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}

