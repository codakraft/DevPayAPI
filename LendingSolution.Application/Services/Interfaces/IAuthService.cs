using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> Login(LoginRequestDto body);
    Task<ApiResponse> CreateSuperAdmin(CreateSuperAdminRequestDto body);
    Task<ApiResponse> CreateAdmin(CreateAdminRequestDto body);
    Task<ApiResponse> AdminLogin(LoginRequestDto body);
    ApiResponse VerifyOtp(VerifyOtpRequestDto body);
    ApiResponse SalaryHistoryReview(ReviewHistoryRequestDto body);
    Task<ApiResponse> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user);
}
