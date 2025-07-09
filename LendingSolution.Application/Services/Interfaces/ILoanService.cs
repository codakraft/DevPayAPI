using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Interfaces;

public interface ILoanService
{
    Task<string> Register(RegisterRequestDto body);
    Task<Loan> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user);
    Task<Loan> ApproveLoan(Guid loanId);
    Task<List<Loan>> GetAllLoans();
    
    // New methods for comprehensive loan management
    Task<PagedLoanListDto> GetAllLoansAsync(LoanFilterDto filter); // For SuperAdmin - all companies
    Task<PagedLoanListDto> GetCompanyLoansAsync(Guid companyId, LoanFilterDto filter); // For Admin/SuperAdmin - specific company
    Task<LoanListDto> GetLoanByIdAsync(Guid loanId, string? requestingUserId = null); // Get single loan with access control
    
    // ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body);
    // Task<ApiResponse> SubmitLoan(Guid loanId, SubmitRequestDto body);
    // Task<ApiResponse> SalaryHistoryReview(ReviewHistoryRequestDto body);
}