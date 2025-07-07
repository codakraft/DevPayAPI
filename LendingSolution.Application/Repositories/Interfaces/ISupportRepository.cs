using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(string id);
    Task<SupportTicket?> GetByIdWithCommentsAsync(string id);
    Task<IEnumerable<SupportTicket>> GetAllAsync();
    Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId);
    Task<IEnumerable<SupportTicket>> GetByCompanyIdAsync(Guid companyId);
    Task<IEnumerable<SupportTicket>> GetByStatusAsync(string status);
    Task<IEnumerable<SupportTicket>> GetByCategoryAsync(string category);
    Task<IEnumerable<SupportTicket>> GetByPriorityAsync(string priority);
    Task<IEnumerable<SupportTicket>> GetByAssignedToAsync(string assignedTo);
    Task<IEnumerable<SupportTicket>> GetRecentTicketsAsync(int count = 10);
    Task<IEnumerable<SupportTicket>> GetOverdueTicketsAsync();
    Task<SupportTicket> CreateAsync(SupportTicket ticket);
    Task<SupportTicket> UpdateAsync(SupportTicket ticket);
    Task<bool> DeleteAsync(string id);
    Task<int> GetTicketCountByStatusAsync(string status);
    Task<int> GetTicketCountByPriorityAsync(string priority);
    Task<double> GetAverageResolutionTimeHoursAsync();
}

public interface ISupportCommentRepository
{
    Task<SupportComment?> GetByIdAsync(string id);
    Task<IEnumerable<SupportComment>> GetByTicketIdAsync(string ticketId);
    Task<SupportComment> CreateAsync(SupportComment comment);
    Task<SupportComment> UpdateAsync(SupportComment comment);
    Task<bool> DeleteAsync(string id);
}
