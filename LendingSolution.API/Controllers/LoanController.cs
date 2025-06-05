using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LendingSolution.Core.Models.Response;

namespace LendingSolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoanController(ILoanService loanService) : Controller
{
    private readonly ILoanService _loanService = loanService;

    [HttpGet("my")]
    public async Task<IActionResult> MyLoans()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { Success = false, Message = "User not found" });
        }
        var result = await _loanService.GetUserLoans(userId);
        return Ok(result);
    }

    [HttpPost("breakdown")]
    public IActionResult GetLoanBreakdown([FromBody] LoanBreakdownRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = _loanService.GetLoanBreakdown(body);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitLoan([FromBody] SubmitRequestDto body)
    {
        if (body == null || body.LoanId == Guid.Empty)
        {
            return BadRequest(new { Success = false, Message = "Invalid loan ID" });
        }
        var result = await _loanService.SubmitLoan(body.LoanId, User);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("salary-history-review")]
    public async Task<IActionResult> SalaryHistoryReview([FromBody] ReviewHistoryRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = await _loanService.SalaryHistoryReview(body);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}