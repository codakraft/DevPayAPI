using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/loan")]
public class LoanController(
    ILoanService loanService,
    ILogger<LoanController> logger
) : Controller
{
    private readonly ILoanService _loanService = loanService;
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
    // [GET]    /api/loans  
    // [GET]    /api/loans/{id}  
    // [POST]   /api/loans/apply  
    // [PUT]    /api/loans/{id}/edit  
    // [POST]   /api/loans/{id}/approve  
    // [POST]   /api/loans/{id}/reject  
    // [POST]   /api/loans/{id}/disburse  
    // [GET]    /api/loans/user/{userId}  
    // [GET]    /api/loans/company/{companyId}
    // [GET]    /api/loans/status/{status}  
}