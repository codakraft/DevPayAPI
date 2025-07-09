using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class CombinedRepository(ApplicationDbContext db) : ICombinedRepository
{
    ApplicationDbContext _db = db;
    public async Task<Loan?> GetAllLoanInfoByLoanId(Guid loanId)
    {
        return await _db.Loans.Include(l => l.Account)
            .Include(l => l.Product)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == loanId);
    }

    public async Task<bool> UpdateLoanAsync(Loan loan)
    {
        try
        {
            _db.Loans.Update(loan);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<Loan?> GetLoanByMandateIdAsync(string mandateId)
    {
        return await _db.Loans.Include(l => l.Account)
            .Include(l => l.Product)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.MandateId == mandateId);
    }

    public async Task<Loan?> GetLoanByTransactionRefAsync(string transactionRef)
    {
        return await _db.Loans.Include(l => l.Account)
            .Include(l => l.Product)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.DisbursementReference == transactionRef || l.RemitaTransRef == transactionRef);
    }

    public async Task<bool> CreateRepaymentAsync(Repayment repayment)
    {
        try
        {
            _db.Repayments.Add(repayment);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}