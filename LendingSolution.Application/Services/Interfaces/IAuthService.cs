using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuthService
{
    Task<object> Login(LoginRequestDto body);
    Task<object> CreateSuperAdmin(CreateSuperAdminRequestDto body);
    Task<object> CreateAdmin(CreateAdminRequestDto body);
    Task<object> AdminLogin(LoginRequestDto body);
    bool VerifyOtp(VerifyOtpRequestDto body);
    Task<bool> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user);
    Task<object> GetRoles();
    Task<object> AssignRole(RoleAssignDto body);
    Task<object> RefreshToken(RefreshTokenRequestDto request);
    Task<bool> RevokeToken(RevokeTokenRequestDto request, string? userId = null);
    Task<bool> Logout(string? userId = null);
    Task<object> GetSuperAdminDashboardAsync();
    Task<object> GetAdminListAsync(AdminFilterDto filter);
    
    // MFA methods
    Task<AdminLoginResponseDto> AdminLoginWithMfaAsync(LoginRequestDto body);
    Task<VerifyAdminLoginResponseDto> VerifyAdminLoginOtpAsync(VerifyAdminLoginRequestDto request);

    // Company user management
    Task<CreateCompanyUserResponseDto> CreateCompanyUserAsync(CreateCompanyUserRequestDto body, Guid companyId, string createdByUserId);
}
