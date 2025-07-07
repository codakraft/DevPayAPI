using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IDisbursementRepository
{
    Task<bool> CreateDisbursement(Disbursement disbursement);
    Task<List<Disbursement>> GetAllDisbursements();
    Task<List<Disbursement>> GetDisbursementsByLoanId(string loanId);
    Task<Disbursement?> GetDisbursementById(string id);
    Task<bool> UpdateDisbursement(Disbursement disbursement);
    Task<List<Disbursement>> GetDisbursementsByStatus(string status);
    Task<List<Disbursement>> GetDisbursementsByDateRange(DateTime startDate, DateTime endDate);
}
