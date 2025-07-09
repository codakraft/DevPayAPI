using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface ISupportService
{
    // Ticket Management
    Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, string userId);
    Task<List<SupportTicketDto>> GetAllTicketsAsync();
    Task<SupportTicketDto> GetTicketByIdAsync(string ticketId);
    Task UpdateTicketAsync(string ticketId, UpdateSupportTicketDto dto, string userId);
    Task<SupportCommentDto> AddCommentToTicketAsync(string ticketId, AddSupportCommentDto dto, string userId);
    Task<List<SupportTicketDto>> GetTicketsByUserIdAsync(string userId);
    
    // User Account Support
    Task<UserAccountSupportDto> GetUserAccountDetailsAsync(string userId);
    Task<object> SearchUsersAsync(string searchTerm);
    Task PerformUserActionAsync(string userId, UserActionDto dto, string performedBy);
    Task<List<LoanSupportDto>> GetUserLoansAsync(string userId);
    
    // Loan Support
    Task<LoanSupportDto> GetLoanDetailsAsync(string loanId);
    Task<object> SearchLoansAsync(string searchTerm);
    Task<object> GetLoansByStatusAsync(string status);
    Task<object> GetOverdueLoansAsync();
    
    // Support Dashboard
    Task<SupportDashboardDto> GetSupportDashboardAsync();
    Task<List<SupportTicketDto>> GetTicketsByStatusAsync(string status);
    Task<List<SupportTicketDto>> GetTicketsByCategoryAsync(string category);
    Task<List<SupportTicketDto>> GetTicketsByPriorityAsync(string priority);
    
    // Company Admin Dashboard
    Task<CompanyDashboardDto> GetCompanyDashboardAsync(string companyId);
}
