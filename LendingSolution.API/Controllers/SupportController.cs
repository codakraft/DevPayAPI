using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/support")]
[Authorize(Roles = "SupportAgent")]
public class SupportController : Controller
{
    private readonly ISupportService _supportService;
    private readonly ILogger<SupportController> _logger;

    public SupportController(ISupportService supportService, ILogger<SupportController> logger)
    {
        _supportService = supportService;
        _logger = logger;
    }

    // Support Dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetSupportDashboard()
    {
        try
        {
            var result = await _supportService.GetSupportDashboardAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching support dashboard.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Ticket Management
    [HttpGet("tickets")]
    public async Task<IActionResult> GetAllTickets()
    {
        try
        {
            var result = await _supportService.GetAllTicketsAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching support tickets.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/{ticketId}")]
    public async Task<IActionResult> GetTicketById(string ticketId)
    {
        try
        {
            var result = await _supportService.GetTicketByIdAsync(ticketId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching support ticket {TicketId}.", ticketId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _supportService.CreateTicketAsync(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating support ticket.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPut("tickets/{ticketId}")]
    public async Task<IActionResult> UpdateTicket(string ticketId, [FromBody] UpdateSupportTicketDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _supportService.UpdateTicketAsync(ticketId, request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating support ticket {TicketId}.", ticketId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("tickets/{ticketId}/comments")]
    public async Task<IActionResult> AddCommentToTicket(string ticketId, [FromBody] AddSupportCommentDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _supportService.AddCommentToTicketAsync(ticketId, request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding comment to ticket {TicketId}.", ticketId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/status/{status}")]
    public async Task<IActionResult> GetTicketsByStatus(string status)
    {
        try
        {
            var result = await _supportService.GetTicketsByStatusAsync(status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets by status {Status}.", status);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/category/{category}")]
    public async Task<IActionResult> GetTicketsByCategory(string category)
    {
        try
        {
            var result = await _supportService.GetTicketsByCategoryAsync(category);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets by category {Category}.", category);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/priority/{priority}")]
    public async Task<IActionResult> GetTicketsByPriority(string priority)
    {
        try
        {
            var result = await _supportService.GetTicketsByPriorityAsync(priority);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets by priority {Priority}.", priority);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // User Account Management
    [HttpGet("users/search")]
    public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(ApiResponse.Fail("Search term is required"));
            }

            var result = await _supportService.SearchUsersAsync(searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching users.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserAccountDetails(string userId)
    {
        try
        {
            var result = await _supportService.GetUserAccountDetailsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user account details for {UserId}.", userId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("users/{userId}/tickets")]
    public async Task<IActionResult> GetUserTickets(string userId)
    {
        try
        {
            var result = await _supportService.GetTicketsByUserIdAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets for user {UserId}.", userId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("users/{userId}/loans")]
    public async Task<IActionResult> GetUserLoans(string userId)
    {
        try
        {
            var result = await _supportService.GetUserLoansAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loans for user {UserId}.", userId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("users/{userId}/actions")]
    public async Task<IActionResult> PerformUserAction(string userId, [FromBody] UserActionDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var performedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _supportService.PerformUserActionAsync(userId, request, performedBy);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while performing action on user {UserId}.", userId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Loan Support
    [HttpGet("loans/search")]
    public async Task<IActionResult> SearchLoans([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(ApiResponse.Fail("Search term is required"));
            }

            var result = await _supportService.SearchLoansAsync(searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("loans/{loanId}")]
    public async Task<IActionResult> GetLoanDetails(string loanId)
    {
        try
        {
            var result = await _supportService.GetLoanDetailsAsync(loanId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loan details for {LoanId}.", loanId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("loans/status/{status}")]
    public async Task<IActionResult> GetLoansByStatus(string status)
    {
        try
        {
            var result = await _supportService.GetLoansByStatusAsync(status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loans by status {Status}.", status);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("loans/overdue")]
    public async Task<IActionResult> GetOverdueLoans()
    {
        try
        {
            var result = await _supportService.GetOverdueLoansAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching overdue loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}