using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<TokenResponseDto> GenerateTokenWithRefreshAsync(ApplicationUser user);
    Task<string> GenerateRefreshTokenAsync();
    Task<TokenResponseDto> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken, string? revokedBy = null, string? reason = null);
    Task<bool> RevokeAllUserTokensAsync(string userId, string? revokedBy = null, string? reason = null);
    Task<bool> IsRefreshTokenValidAsync(string refreshToken);
}