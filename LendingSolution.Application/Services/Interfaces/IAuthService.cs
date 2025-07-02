using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse> Login(LoginRequestDto body);
    Task<ApiResponse> CreateSuperAdmin(CreateSuperAdminRequestDto body);
    Task<ApiResponse> CreateAdmin(CreateAdminRequestDto body);
    Task<object> AdminLogin(LoginRequestDto body);
    bool VerifyOtp(VerifyOtpRequestDto body);
    Task<bool> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user);
    Task<ApiResponse> GetRoles();
    Task<ApiResponse> AssignRole(RoleAssignDto body);
}
