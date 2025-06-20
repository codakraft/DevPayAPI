using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Interfaces;

public interface ILoanService
{
    Task<ApiResponse> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user);
    Task<ApiResponse> GetUserLoans(string userId);
    Task<ApiResponse> ApproveLoan(Guid loanId);
    Task<ApiResponse> GetAllLoans();
    ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body);
    Task<ApiResponse> SubmitLoan(Guid loanId, ClaimsPrincipal user);
    Task<ApiResponse<ReviewHistoryResponseDto>> SalaryHistoryReview(ReviewHistoryRequestDto body);
}