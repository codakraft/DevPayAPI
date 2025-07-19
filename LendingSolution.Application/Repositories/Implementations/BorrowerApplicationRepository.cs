using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class BorrowerApplicationRepository : IBorrowerApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public BorrowerApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BorrowerApplication> CreateAsync(BorrowerApplication application)
    {
        _context.BorrowerApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task<BorrowerApplication?> GetByIdAsync(Guid id)
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .FirstOrDefaultAsync(ba => ba.Id == id);
    }

    public async Task<BorrowerApplication?> GetByEmailAsync(string email)
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .FirstOrDefaultAsync(ba => ba.Email == email && ba.IsActive);
    }

    public async Task<BorrowerApplication?> GetByBvnAsync(string bvn)
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .FirstOrDefaultAsync(ba => ba.BVN == bvn && ba.IsActive);
    }

    public async Task<BorrowerApplication?> GetByLoanIdAsync(Guid loanId)
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .FirstOrDefaultAsync(ba => ba.LoanId == loanId);
    }

    public async Task<bool> UpdateAsync(BorrowerApplication application)
    {
        _context.BorrowerApplications.Update(application);
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var application = await _context.BorrowerApplications.FindAsync(id);
        if (application == null) return false;

        application.IsActive = false;
        var result = await _context.SaveChangesAsync();
        return result > 0;
    }

    public async Task<List<BorrowerApplication>> GetByCompanyIdAsync(Guid companyId)
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .Where(ba => ba.CompanyId == companyId && ba.IsActive)
            .OrderByDescending(ba => ba.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BorrowerApplication>> GetByStepAsync(BorrowerOnboardingStep step)
    {
        return await _context.BorrowerApplications
            .Include(ba => ba.Company)
            .Include(ba => ba.Product)
            .Include(ba => ba.Loan)
            .Where(ba => ba.CurrentStep == step && ba.IsActive)
            .OrderByDescending(ba => ba.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.BorrowerApplications
            .AnyAsync(ba => ba.Email == email && ba.IsActive && !ba.IsCompleted);
    }
}
