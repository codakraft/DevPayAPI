using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class RepaymentRepository : IRepaymentRepository
{
    private readonly ApplicationDbContext _db;

    public RepaymentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CreateRepayment(Repayment repayment)
    {
        _db.Repayments.Add(repayment);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<Repayment>> GetAllRepayments()
    {
        return await _db.Repayments
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Repayment>> GetRepaymentsByLoanId(string loanId)
    {
        return await _db.Repayments
            .Where(r => r.LoanId == Guid.Parse(loanId))
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Repayment?> GetRepaymentById(string id)
    {
        return await _db.Repayments
            .FirstOrDefaultAsync(r => r.Id.ToString() == id);
    }

    public async Task<bool> UpdateRepayment(Repayment repayment)
    {
        _db.Repayments.Update(repayment);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<Repayment>> GetRepaymentsByStatus(string status)
    {
        var statusEnum = Enum.Parse<Core.Enum.RepaymentStatus>(status, true);
        return await _db.Repayments
            .Where(r => r.Status == statusEnum)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Repayment>> GetRepaymentsByDateRange(DateTime startDate, DateTime endDate)
    {
        return await _db.Repayments
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRepaymentsByLoanId(string loanId)
    {
        var repayment = await _db.Repayments
            .FirstOrDefaultAsync(r => r.LoanId == Guid.Parse(loanId));
        return repayment?.TotalRepaid ?? 0;
    }
}
