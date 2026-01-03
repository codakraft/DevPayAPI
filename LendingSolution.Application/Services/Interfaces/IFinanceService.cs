using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IFinanceService
{
    Task<List<DisbursementDto>> GetAllDisbursementsAsync();
    Task<List<DisbursementDto>> GetCompanyDisbursementsAsync(Guid companyId);
    Task<List<RepaymentDto>> GetAllRepaymentsAsync();
    Task<DisbursementDto> ProcessDisbursementAsync(string loanId, DisbursementRequestDto request, string? processedBy = null);
    Task<RepaymentDto> ProcessRepaymentAsync(RepaymentRequestDto request, string? processedBy = null);
    Task<FinanceReportDto> GetMonthlyFinanceReportAsync(int year, int month);
    Task<FinanceReportDto> GetCompanyFinanceReportAsync(string companyId);
    Task<CompanyWalletDto> GetCompanyWalletAsync(string companyId);
    Task<List<DisbursementDto>> GetDisbursementsByLoanIdAsync(string loanId);
    Task<List<RepaymentDto>> GetRepaymentsByLoanIdAsync(string loanId);
}
