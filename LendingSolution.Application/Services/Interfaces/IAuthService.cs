using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAuthService
{
    Task<object> Login(LoginRequestDto body);
    Task<object> CreateSuperAdmin(CreateSuperAdminRequestDto body, string? callerId, bool callerIsSuperAdmin);
    Task<object> CreateAdmin(CreateAdminRequestDto body, string callerId);
    Task<object> AdminLogin(LoginRequestDto body);
    bool VerifyOtp(VerifyOtpRequestDto body);
    Task<bool> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user);
    Task<object> GetRoles();
    Task<object> AssignRole(RoleAssignDto body, System.Security.Claims.ClaimsPrincipal caller);
    Task<object> RemoveRole(RoleAssignDto body, System.Security.Claims.ClaimsPrincipal caller);
    Task<object> SetUserActiveAsync(string userId, bool isActive, System.Security.Claims.ClaimsPrincipal caller);
    Task<object> UpdateUserAsync(string userId, UpdateUserRequestDto body, System.Security.Claims.ClaimsPrincipal caller);
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

    // Password management
    Task<TokenResponseDto> ChangePasswordAsync(ChangePasswordRequestDto body, string userId);
    Task RequestAdminPasswordResetAsync(string email);
    Task ResetAdminPasswordAsync(AdminResetPasswordRequestDto body);
}
