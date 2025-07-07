using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LendingSolution.Application.Services.Implementations;

public class TokenService(
    UserManager<ApplicationUser> userManager, 
    IOptions<JwtSettings> configuration,
    IRefreshTokenRepository refreshTokenRepository) : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly JwtSettings _jwtSettings = configuration.Value;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id?.ToString() ?? string.Empty),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("FirstName", user.FirstName ?? string.Empty),
                new("LastName", user.LastName ?? string.Empty),
            };

        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(userClaims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public async Task<TokenResponseDto> GenerateTokenWithRefreshAsync(ApplicationUser user)
    {
        var accessToken = await GenerateTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync();
        
        // Save refresh token to database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id!,
            ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryInDays)
        };
        
        await _refreshTokenRepository.CreateTokenAsync(refreshTokenEntity);
        
        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpiryInMinutes),
            RefreshTokenExpiry = refreshTokenEntity.ExpiryDate
        };
    }

    public Task<string> GenerateRefreshTokenAsync()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Task.FromResult(Convert.ToBase64String(randomBytes));
    }

    public async Task<ApiResponse> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            
            if (storedToken == null)
            {
                return ApiResponse.Fail("Invalid refresh token");
            }

            if (!storedToken.IsActive)
            {
                return ApiResponse.Fail("Refresh token is expired or revoked");
            }

            var user = storedToken.User;
            if (user == null)
            {
                return ApiResponse.Fail("User not found");
            }

            // Generate new tokens
            var newTokenResponse = await GenerateTokenWithRefreshAsync(user);
            
            // Revoke the old refresh token
            await _refreshTokenRepository.RevokeTokenAsync(refreshToken, user.Id, "Replaced by new token");
            
            return ApiResponse.Ok("Tokens refreshed successfully", newTokenResponse);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to refresh token: {ex.Message}");
        }
    }

    public async Task<ApiResponse> RevokeTokenAsync(string refreshToken, string? revokedBy = null, string? reason = null)
    {
        try
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            
            if (storedToken == null)
            {
                return ApiResponse.Fail("Invalid refresh token");
            }

            if (!storedToken.IsActive)
            {
                return ApiResponse.Fail("Refresh token is already revoked or expired");
            }

            await _refreshTokenRepository.RevokeTokenAsync(refreshToken, revokedBy, reason ?? "Token revoked by user");
            
            return ApiResponse.Ok("Refresh token revoked successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to revoke token: {ex.Message}");
        }
    }

    public async Task<ApiResponse> RevokeAllUserTokensAsync(string userId, string? revokedBy = null, string? reason = null)
    {
        try
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, revokedBy, reason ?? "All tokens revoked");
            
            return ApiResponse.Ok("All refresh tokens revoked successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to revoke all tokens: {ex.Message}");
        }
    }

    public async Task<bool> IsRefreshTokenValidAsync(string refreshToken)
    {
        return await _refreshTokenRepository.IsTokenActiveAsync(refreshToken);
    }
}