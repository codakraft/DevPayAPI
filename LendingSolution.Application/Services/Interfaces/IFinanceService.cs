using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface IFinanceService
{
    Task<ApiResponse> GetAllDisbursementsAsync();
    Task<ApiResponse> GetAllRepaymentsAsync();
    Task<ApiResponse> ProcessDisbursementAsync(string loanId, DisbursementRequestDto request, string? processedBy = null);
    Task<ApiResponse> ProcessRepaymentAsync(RepaymentRequestDto request, string? processedBy = null);
    Task<ApiResponse> GetMonthlyFinanceReportAsync(int year, int month);
    Task<ApiResponse> GetCompanyFinanceReportAsync(string companyId);
    Task<ApiResponse> GetCompanyWalletAsync(string companyId);
    Task<ApiResponse> GetDisbursementsByLoanIdAsync(string loanId);
    Task<ApiResponse> GetRepaymentsByLoanIdAsync(string loanId);
}
