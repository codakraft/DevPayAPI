using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<bool> CreateLoan(Loan loanRequest);
    Task<Loan?> GetLoanById(Guid loanId);
    Task<IEnumerable<Loan>> GetAllLoans();
    Task<IEnumerable<Loan>> GetAllLoansByCompanyId(Guid CompanyId);
    Task<bool> UpdateLoan(Loan loanRequestDto);
    Task<bool> UpdateLoanStatus(Guid id, LoanStatus loanStatus);
    // Task<bool> DeleteLoan(Guid loanId);
    Task<IEnumerable<Loan>> GetLoansByUserIdAsync(string userId);
}