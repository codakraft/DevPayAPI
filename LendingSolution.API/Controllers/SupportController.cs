using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using LendingSolution.API.Auth;
using LendingSolution.Core.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/support")]
[Authorize]
public class SupportController : Controller
{
    private readonly ISupportService _supportService;
    private readonly ILoanService _loanService;
    private readonly ILogger<SupportController> _logger;

    public SupportController(ISupportService supportService, ILoanService loanService, ILogger<SupportController> logger)
    {
        _supportService = supportService;
        _loanService = loanService;
        _logger = logger;
    }

    /// <summary>
    /// null for SuperAdmin (platform-wide); the caller's company for everyone else.
    /// </summary>
    private Guid? CompanyScope() =>
        User.IsSuperAdmin()
            ? null
            : User.GetCompanyId() ?? throw new AppException("Company ID not found in token", 403);

    // Support Dashboard
    [HttpGet("dashboard")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetSupportDashboard()
    {
        try
        {
            var result = await _supportService.GetSupportDashboardAsync(CompanyScope());
            _logger.LogInformation("Successfully fetched support dashboard");
            return Ok(ApiResponse.Ok("Support dashboard fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching support dashboard.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Ticket Management
    [HttpGet("tickets")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetAllTickets()
    {
        try
        {
            var result = await _supportService.GetAllTicketsAsync(CompanyScope());
            _logger.LogInformation("Successfully fetched all support tickets");
            return Ok(ApiResponse.Ok("Support tickets fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all tickets.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/{ticketId}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetTicketById(string ticketId)
    {
        try
        {
            var result = await _supportService.GetTicketByIdAsync(ticketId, CompanyScope());
            _logger.LogInformation("Successfully fetched support ticket {TicketId}", ticketId);
            return Ok(ApiResponse.Ok("Support ticket fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching ticket by ID.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("tickets")]
    [HasPermission(Permissions.Support.Manage)]
    public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _supportService.CreateTicketAsync(request, userId);
            _logger.LogInformation("Successfully created support ticket");
            return Ok(ApiResponse.Ok("Support ticket created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating support ticket.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPut("tickets/{ticketId}")]
    [HasPermission(Permissions.Support.Manage)]
    public async Task<IActionResult> UpdateTicket(string ticketId, [FromBody] UpdateSupportTicketDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _supportService.UpdateTicketAsync(ticketId, request, userId);
            _logger.LogInformation("Successfully updated support ticket {TicketId}", ticketId);
            return Ok(ApiResponse.Ok("Support ticket updated successfully"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating support ticket.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("tickets/{ticketId}/comments")]
    [HasPermission(Permissions.Support.Manage)]
    public async Task<IActionResult> AddCommentToTicket(string ticketId, [FromBody] AddSupportCommentDto request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var result = await _supportService.AddCommentToTicketAsync(ticketId, request, userId, CompanyScope());
            _logger.LogInformation("Successfully added comment to ticket {TicketId}", ticketId);
            return Ok(ApiResponse.Ok("Comment added to support ticket successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding comment to ticket.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/status/{status}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetTicketsByStatus(string status)
    {
        try
        {
            var result = await _supportService.GetTicketsByStatusAsync(status, CompanyScope());
            _logger.LogInformation("Successfully fetched tickets by status {Status}", status);
            return Ok(ApiResponse.Ok("Tickets fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets by status.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/category/{category}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetTicketsByCategory(string category)
    {
        try
        {
            var result = await _supportService.GetTicketsByCategoryAsync(category, CompanyScope());
            _logger.LogInformation("Successfully fetched tickets by category {Category}", category);
            return Ok(ApiResponse.Ok("Tickets fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets by category.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("tickets/priority/{priority}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetTicketsByPriority(string priority)
    {
        try
        {
            var result = await _supportService.GetTicketsByPriorityAsync(priority, CompanyScope());
            _logger.LogInformation("Successfully fetched tickets by priority {Priority}", priority);
            return Ok(ApiResponse.Ok("Tickets fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets by priority.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // User Account Management
    [HttpGet("users/search")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
    {
        try
        {
            var result = await _supportService.SearchUsersAsync(searchTerm, CompanyScope());
            _logger.LogInformation("Successfully searched users with term: {SearchTerm}", searchTerm);
            return Ok(ApiResponse.Ok("User search results", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching users.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("users/{userId}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetUserAccountDetails(string userId)
    {
        try
        {
            var result = await _supportService.GetUserAccountDetailsAsync(userId, CompanyScope());
            _logger.LogInformation("Successfully fetched user account details for {UserId}", userId);
            return Ok(ApiResponse.Ok("User account details fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user account details.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("users/{userId}/tickets")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetUserTickets(string userId)
    {
        try
        {
            var result = await _supportService.GetTicketsByUserIdAsync(userId);
            _logger.LogInformation("Successfully fetched tickets for user {UserId}", userId);
            return Ok(ApiResponse.Ok("User tickets fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching tickets for user.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("users/{userId}/loans")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetUserLoans(string userId)
    {
        try
        {
            var result = await _supportService.GetUserLoansAsync(userId, CompanyScope());
            _logger.LogInformation("Successfully fetched loans for user {UserId}", userId);
            return Ok(ApiResponse.Ok("User loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loans for user.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpPost("users/{userId}/actions")]
    // Platform-wide account actions; company admins use admin/users/{id}/activate|deactivate
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PerformUserAction(string userId, [FromBody] UserActionDto request)
    {
        try
        {
            var performedBy = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            await _supportService.PerformUserActionAsync(userId, request, performedBy);
            _logger.LogInformation("Successfully performed action on user {UserId}", userId);
            return Ok(ApiResponse.Ok("User action performed successfully"));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while performing action on user.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Loan Support
    [HttpGet("loans/search")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> SearchLoans([FromQuery] string searchTerm)
    {
        try
        {
            var result = await _supportService.SearchLoansAsync(searchTerm, CompanyScope());
            _logger.LogInformation("Successfully searched loans with term: {SearchTerm}", searchTerm);
            return Ok(ApiResponse.Ok("Loan search results", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("loans/{loanId}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetLoanDetails(string loanId)
    {
        try
        {
            if (!Guid.TryParse(loanId, out var loanGuid) || !await User.CanAccessLoanAsync(_loanService, loanGuid))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

            var result = await _supportService.GetLoanDetailsAsync(loanId);
            _logger.LogInformation("Successfully fetched loan details for {LoanId}", loanId);
            return Ok(ApiResponse.Ok("Loan details fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loan details.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [HttpGet("loans/status/{status}")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetLoansByStatus(string status)
    {
        try
        {
            var result = await _supportService.GetLoansByStatusAsync(status, CompanyScope());
            _logger.LogInformation("Successfully fetched loans by status {Status}", status);
            return Ok(ApiResponse.Ok("Loans fetched successfully", result));
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

    [HttpGet("loans/overdue")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetOverdueLoans()
    {
        try
        {
            var result = await _supportService.GetOverdueLoansAsync(CompanyScope());
            _logger.LogInformation("Successfully fetched overdue loans");
            return Ok(ApiResponse.Ok("Overdue loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching overdue loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Company Dashboard for Support
    [HttpGet("companies/{companyId}/dashboard")]
    [HasPermission(Permissions.Support.View)]
    public async Task<IActionResult> GetCompanyDashboard(string companyId)
    {
        try
        {
            if (!Guid.TryParse(companyId, out var companyGuid) || !User.CanAccessCompany(companyGuid))
            {
                return StatusCode(403, ApiResponse.Fail("You can only access your own company's data"));
            }

            var result = await _supportService.GetCompanyDashboardAsync(companyId);
            _logger.LogInformation("Successfully fetched company dashboard for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company dashboard fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message).WithCode(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company dashboard.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }
}