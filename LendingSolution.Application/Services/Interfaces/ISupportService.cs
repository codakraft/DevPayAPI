using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface ISupportService
{
    // Ticket Management
    Task<ApiResponse> CreateTicketAsync(CreateSupportTicketDto dto, string userId);
    Task<ApiResponse> GetAllTicketsAsync();
    Task<ApiResponse> GetTicketByIdAsync(string ticketId);
    Task<ApiResponse> UpdateTicketAsync(string ticketId, UpdateSupportTicketDto dto, string userId);
    Task<ApiResponse> AddCommentToTicketAsync(string ticketId, AddSupportCommentDto dto, string userId);
    Task<ApiResponse> GetTicketsByUserIdAsync(string userId);
    
    // User Account Support
    Task<ApiResponse> GetUserAccountDetailsAsync(string userId);
    Task<ApiResponse> SearchUsersAsync(string searchTerm);
    Task<ApiResponse> PerformUserActionAsync(string userId, UserActionDto dto, string performedBy);
    Task<ApiResponse> GetUserLoansAsync(string userId);
    
    // Loan Support
    Task<ApiResponse> GetLoanDetailsAsync(string loanId);
    Task<ApiResponse> SearchLoansAsync(string searchTerm);
    Task<ApiResponse> GetLoansByStatusAsync(string status);
    Task<ApiResponse> GetOverdueLoansAsync();
    
    // Support Dashboard
    Task<ApiResponse> GetSupportDashboardAsync();
    Task<ApiResponse> GetTicketsByStatusAsync(string status);
    Task<ApiResponse> GetTicketsByCategoryAsync(string category);
    Task<ApiResponse> GetTicketsByPriorityAsync(string priority);
    
    // Company Admin Dashboard
    Task<ApiResponse> GetCompanyDashboardAsync(string companyId);
}
