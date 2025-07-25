using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Interfaces;

public interface ILoanService
{
    Task<string> Register(RegisterRequestDto body);
    Task<Loan> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user);
    Task<Loan> ApproveLoan(Guid loanId, string? approvedBy = null, string? reason = null);
    Task<Loan> RejectLoan(Guid loanId, string? rejectedBy = null, string? reason = null);
    Task<List<Loan>> GetAllLoans();
    Task<List<Loan>> GetPendingLoans(); // Replaces GetPendingApprovalsAsync
    Task<List<Loan>> GetLoansByStatus(LoanStatus status);
    Task<Loan> ProcessLoan(Guid loanId, ProcessLoanRequestDto request, string processedBy);
    
    // New methods for comprehensive loan management
    Task<PagedLoanListDto> GetAllLoansAsync(LoanFilterDto filter); // For SuperAdmin - all companies
    Task<PagedLoanListDto> GetCompanyLoansAsync(Guid companyId, LoanFilterDto filter); // For Admin/SuperAdmin - specific company
    Task<LoanListDto> GetLoanByIdAsync(Guid loanId, string? requestingUserId = null); // Get single loan with access control
    
    // ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body);
    // Task<ApiResponse> SubmitLoan(Guid loanId, SubmitRequestDto body);
    // Task<ApiResponse> SalaryHistoryReview(ReviewHistoryRequestDto body);
}