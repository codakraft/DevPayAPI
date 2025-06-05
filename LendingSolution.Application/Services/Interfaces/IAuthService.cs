using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> Register(RegisterRequestDto body);
    Task<ApiResponse> Login(LoginRequestDto body);
    ApiResponse VerifyOtp(VerifyOtpRequestDto body);
    ApiResponse SalaryHistoryReview(ReviewHistoryRequestDto body);
    Task<ApiResponse> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user);
}
