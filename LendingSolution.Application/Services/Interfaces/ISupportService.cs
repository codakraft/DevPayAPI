using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

/// <remarks>
/// <c>companyScope</c>: null = platform-wide (SuperAdmin); otherwise results are limited to that company
/// and records from other companies are reported as not found.
/// </remarks>
public interface ISupportService
{
    // Ticket Management
    Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, string userId);
    Task<List<SupportTicketDto>> GetAllTicketsAsync(Guid? companyScope = null);
    Task<SupportTicketDto> GetTicketByIdAsync(string ticketId, Guid? companyScope = null);
    Task UpdateTicketAsync(string ticketId, UpdateSupportTicketDto dto, string userId);
    Task<SupportCommentDto> AddCommentToTicketAsync(string ticketId, AddSupportCommentDto dto, string userId, Guid? companyScope = null);
    Task<List<SupportTicketDto>> GetTicketsByUserIdAsync(string userId);
    
    // User Account Support
    Task<UserAccountSupportDto> GetUserAccountDetailsAsync(string userId, Guid? companyScope = null);
    Task<object> SearchUsersAsync(string searchTerm, Guid? companyScope = null);
    Task PerformUserActionAsync(string userId, UserActionDto dto, string performedBy);
    Task<List<LoanSupportDto>> GetUserLoansAsync(string userId, Guid? companyScope = null);
    
    // Loan Support
    Task<LoanSupportDto> GetLoanDetailsAsync(string loanId);
    Task<object> SearchLoansAsync(string searchTerm, Guid? companyScope = null);
    Task<object> GetLoansByStatusAsync(string status, Guid? companyScope = null);
    Task<object> GetOverdueLoansAsync(Guid? companyScope = null);
    
    // Support Dashboard
    Task<SupportDashboardDto> GetSupportDashboardAsync(Guid? companyScope = null);
    Task<List<SupportTicketDto>> GetTicketsByStatusAsync(string status, Guid? companyScope = null);
    Task<List<SupportTicketDto>> GetTicketsByCategoryAsync(string category, Guid? companyScope = null);
    Task<List<SupportTicketDto>> GetTicketsByPriorityAsync(string priority, Guid? companyScope = null);
    
    // Company Admin Dashboard
    Task<CompanyDashboardDto> GetCompanyDashboardAsync(string companyId);
}
