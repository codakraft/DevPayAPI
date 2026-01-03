using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class LoanRespository(ApplicationDbContext db) : ILoanRepository
{
    private ApplicationDbContext _db = db;

    public Task<bool> CreateLoan(Loan loanRequest)
    {
        _db.Loans.Add(loanRequest);
        return _db.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public Task<IEnumerable<Loan>> GetAllLoans()
    {
        return Task.FromResult(_db.Loans.AsEnumerable());
    }

    public Task<IEnumerable<Loan>> GetAllLoansByCompanyId(Guid CompanyId)
    {
        var loans = _db.Loans.Where(loan => loan.CompanyId == CompanyId);
        return Task.FromResult(loans.AsEnumerable());
    }

    public Task<Loan?> GetLoanById(Guid loanId)
    {
        var loan = _db.Loans.FirstOrDefaultAsync(loan => loan.Id == loanId);
        return loan;
    }

    public Task<IEnumerable<Loan>> GetLoansByUserIdAsync(string userId)
    {
        var loans = _db.Loans.Where(loan => loan.UserId == userId);
        return Task.FromResult(loans.AsEnumerable());
    }

    public Task<bool> UpdateLoan(Loan loanRequestDto)
    {
        _db.Loans.Update(loanRequestDto);
        return _db.SaveChangesAsync().ContinueWith(t => t.Result > 0);
    }

    public Task<bool> UpdateLoanStatus(Guid id, LoanStatus loanStatus)
    {
        var loan = _db.Loans.FirstOrDefaultAsync(loan => loan.Id == id);
        if (loan is null)
        {
            return Task.FromResult(false);
        }

        if (loan.Result != null)
        {
            loan.Result.Status = loanStatus;
            _db.Loans.Update(loan.Result);
            return _db.SaveChangesAsync().ContinueWith(t => t.Result > 0);
        }
        return Task.FromResult(false);
    }

    public async Task<Loan?> GetLoanByIdWithIncludes(Guid loanId)
    {
        return await _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
                .ThenInclude(ba => ba!.RemitaSalaryHistory)
            .FirstOrDefaultAsync(l => l.Id == loanId);
    }

    public async Task<List<Loan>> GetPendingLoans()
    {
        return await _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
            .Where(l => l.Status == LoanStatus.Pending)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetLoansByStatus(LoanStatus status)
    {
        return await _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
            .Where(l => l.Status == status)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetAllLoansWithIncludes()
    {
        return await _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public IQueryable<Loan> GetAllLoansQueryable()
    {
        return _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
            .AsQueryable();
    }

    public IQueryable<Loan> GetCompanyLoansQueryable(Guid companyId)
    {
        return _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
                .ThenInclude(ba => ba != null ? ba.RemitaSalaryHistory : null)
            .Where(l => l.CompanyId == companyId)
            .AsQueryable();
    }
}