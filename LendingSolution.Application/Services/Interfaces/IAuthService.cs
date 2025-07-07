using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> Login(LoginRequestDto body);
    Task<object> CreateSuperAdmin(CreateSuperAdminRequestDto body);
    Task<object> CreateAdmin(CreateAdminRequestDto body);
    Task<object> AdminLogin(LoginRequestDto body);
    bool VerifyOtp(VerifyOtpRequestDto body);
    Task<bool> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user);
    Task<ApiResponse> GetRoles();
    Task<ApiResponse> AssignRole(RoleAssignDto body);
    Task<ApiResponse> RefreshToken(RefreshTokenRequestDto request);
    Task<ApiResponse> RevokeToken(RevokeTokenRequestDto request, string? userId = null);
    Task<ApiResponse> Logout(string? userId = null);
    Task<ApiResponse> GetSuperAdminDashboardAsync();
    Task<ApiResponse> GetAdminListAsync(AdminFilterDto filter);
}
