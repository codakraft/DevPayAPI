using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class LoanProductRepository : ILoanProductRepository
{
    private readonly ApplicationDbContext _db;

    public LoanProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CreateLoanProduct(LoanProduct loanProduct)
    {
        _db.LoanProducts.Add(loanProduct);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<LoanProduct>> GetLoanProductsByCompanyId(Guid companyId)
    {
        return await _db.LoanProducts
            .Where(lp => lp.CompanyId == companyId)
            .ToListAsync();
    }

    public async Task<LoanProduct?> GetLoanProductById(Guid id)
    {
        return await _db.LoanProducts.FirstOrDefaultAsync(lp => lp.Id == id);
    }

    public async Task<bool> UpdateLoanProduct(LoanProduct loanProduct)
    {
        _db.LoanProducts.Update(loanProduct);
        var result = await _db.SaveChangesAsync();
        return result > 0;
    }
}