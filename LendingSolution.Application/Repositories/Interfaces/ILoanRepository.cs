using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<bool> CreateLoan(Loan loanRequest);
    Task<Loan?> GetLoanById(Guid loanId);
    Task<Loan?> GetLoanByIdWithIncludes(Guid loanId); // With User, Company, Product, BorrowerApplication
    Task<IEnumerable<Loan>> GetAllLoans();
    Task<IEnumerable<Loan>> GetAllLoansByCompanyId(Guid CompanyId);
    Task<bool> UpdateLoan(Loan loanRequestDto);
    Task<bool> UpdateLoanStatus(Guid id, LoanStatus loanStatus);
    Task<IEnumerable<Loan>> GetLoansByUserIdAsync(string userId);
    Task<List<Loan>> GetPendingLoans(); // Get loans with Pending status
    Task<List<Loan>> GetLoansByStatus(LoanStatus status); // Get loans by specific status
    Task<List<Loan>> GetAllLoansWithIncludes(); // Get all loans with includes
    
    // New queryable methods for filtering and pagination
    IQueryable<Loan> GetAllLoansQueryable(); // For SuperAdmin filtering
    IQueryable<Loan> GetCompanyLoansQueryable(Guid companyId); // For company-specific filtering
}