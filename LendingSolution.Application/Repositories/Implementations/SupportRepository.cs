using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly ApplicationDbContext _context;

    public SupportTicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupportTicket?> GetByIdAsync(string id)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<SupportTicket?> GetByIdWithCommentsAsync(string id)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<SupportTicket>> GetAllAsync()
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByUserIdAsync(string userId)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.CompanyId == companyId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByStatusAsync(string status)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByCategoryAsync(string category)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Category == category)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByPriorityAsync(string priority)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Priority == priority)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetByAssignedToAsync(string assignedTo)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.AssignedTo == assignedTo)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetRecentTicketsAsync(int count = 10)
    {
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupportTicket>> GetOverdueTicketsAsync()
    {
        var overdueDate = DateTime.UtcNow.AddDays(-7); // Consider tickets older than 7 days as overdue
        return await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Company)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Status != "Resolved" && t.Status != "Closed" && t.CreatedAt < overdueDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportTicket> CreateAsync(SupportTicket ticket)
    {
        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<SupportTicket> UpdateAsync(SupportTicket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        _context.SupportTickets.Update(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var ticket = await _context.SupportTickets.FindAsync(id);
        if (ticket == null) return false;

        _context.SupportTickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetTicketCountByStatusAsync(string status)
    {
        return await _context.SupportTickets
            .CountAsync(t => t.Status == status);
    }

    public async Task<int> GetTicketCountByPriorityAsync(string priority)
    {
        return await _context.SupportTickets
            .CountAsync(t => t.Priority == priority);
    }

    public async Task<double> GetAverageResolutionTimeHoursAsync()
    {
        var resolvedTickets = await _context.SupportTickets
            .Where(t => t.ResolvedAt.HasValue)
            .Select(t => new { t.CreatedAt, t.ResolvedAt })
            .ToListAsync();

        if (!resolvedTickets.Any()) return 0;

        var totalHours = resolvedTickets
            .Sum(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);

        return totalHours / resolvedTickets.Count;
    }
}

public class SupportCommentRepository : ISupportCommentRepository
{
    private readonly ApplicationDbContext _context;

    public SupportCommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SupportComment?> GetByIdAsync(string id)
    {
        return await _context.SupportComments
            .Include(c => c.User)
            .Include(c => c.Ticket)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<SupportComment>> GetByTicketIdAsync(string ticketId)
    {
        return await _context.SupportComments
            .Include(c => c.User)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<SupportComment> CreateAsync(SupportComment comment)
    {
        _context.SupportComments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<SupportComment> UpdateAsync(SupportComment comment)
    {
        _context.SupportComments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var comment = await _context.SupportComments.FindAsync(id);
        if (comment == null) return false;

        _context.SupportComments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}
