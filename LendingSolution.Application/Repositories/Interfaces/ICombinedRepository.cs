using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ICombinedRepository
{
    Task<Loan?> GetAllLoanInfoByLoanId(Guid loanId);
}