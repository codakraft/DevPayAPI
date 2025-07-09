using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ICombinedRepository
{
    Task<Loan?> GetAllLoanInfoByLoanId(Guid loanId);
    
    // Missing methods for Remita integration
    Task<bool> UpdateLoanAsync(Loan loan);
    Task<Loan?> GetLoanByMandateIdAsync(string mandateId);
    Task<Loan?> GetLoanByTransactionRefAsync(string transactionRef);
    Task<bool> CreateRepaymentAsync(Repayment repayment);
}