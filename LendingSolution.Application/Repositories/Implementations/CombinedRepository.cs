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
}