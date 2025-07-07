using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class DisbursementRepository : IDisbursementRepository
{
    private readonly ApplicationDbContext _db;

    public DisbursementRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CreateDisbursement(Disbursement disbursement)
    {
        _db.Disbursements.Add(disbursement);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<Disbursement>> GetAllDisbursements()
    {
        return await _db.Disbursements
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Disbursement>> GetDisbursementsByLoanId(string loanId)
    {
        return await _db.Disbursements
            .Where(d => d.LoanId == loanId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<Disbursement?> GetDisbursementById(string id)
    {
        return await _db.Disbursements
            .FirstOrDefaultAsync(d => d.Id.ToString() == id);
    }

    public async Task<bool> UpdateDisbursement(Disbursement disbursement)
    {
        _db.Disbursements.Update(disbursement);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<Disbursement>> GetDisbursementsByStatus(string status)
    {
        return await _db.Disbursements
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Disbursement>> GetDisbursementsByDateRange(DateTime startDate, DateTime endDate)
    {
        return await _db.Disbursements
            .Where(d => d.CreatedAt >= startDate && d.CreatedAt <= endDate)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}
