using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IRepaymentRepository
{
    Task<bool> CreateRepayment(Repayment repayment);
    Task<List<Repayment>> GetAllRepayments();
    Task<List<Repayment>> GetRepaymentsByLoanId(string loanId);
    Task<Repayment?> GetRepaymentById(string id);
    Task<bool> UpdateRepayment(Repayment repayment);
    Task<List<Repayment>> GetRepaymentsByStatus(string status);
    Task<List<Repayment>> GetRepaymentsByDateRange(DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalRepaymentsByLoanId(string loanId);
}
