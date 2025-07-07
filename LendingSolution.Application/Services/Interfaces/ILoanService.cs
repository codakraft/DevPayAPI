using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Interfaces;

public interface ILoanService
{
    Task<string> Register(RegisterRequestDto body);
    Task<ApiResponse> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user);
    // Task<ApiResponse> GetUserLoans(string userId);
    Task<ApiResponse> ApproveLoan(Guid loanId);
    Task<List<Loan>> GetAllLoans();
    
    // New methods for comprehensive loan management
    Task<ApiResponse> GetAllLoansAsync(LoanFilterDto filter); // For SuperAdmin - all companies
    Task<ApiResponse> GetCompanyLoansAsync(Guid companyId, LoanFilterDto filter); // For Admin/SuperAdmin - specific company
    Task<ApiResponse> GetLoanByIdAsync(Guid loanId, string? requestingUserId = null); // Get single loan with access control
    
    // ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body);
    // Task<ApiResponse> SubmitLoan(Guid loanId, SubmitRequestDto body);
    // Task<ApiResponse> SalaryHistoryReview(ReviewHistoryRequestDto body);
}