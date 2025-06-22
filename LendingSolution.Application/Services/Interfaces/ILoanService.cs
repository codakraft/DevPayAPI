using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Interfaces;

public interface ILoanService
{
    Task<ApiResponse> Register(RegisterRequestDto body);

    Task<ApiResponse> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user);
    // Task<ApiResponse> GetUserLoans(string userId);
    Task<ApiResponse> ApproveLoan(Guid loanId);
    Task<ApiResponse> GetAllLoans();
    // ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body);
    Task<ApiResponse> SubmitLoan(Guid loanId, SubmitRequestDto body);
    Task<ApiResponse> SalaryHistoryReview(ReviewHistoryRequestDto body);
}