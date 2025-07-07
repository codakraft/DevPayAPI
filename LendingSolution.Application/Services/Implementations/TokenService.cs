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
using LendingSolution.Application.Exceptions;

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

        // Add CompanyId claim for Admin users
        if (!string.IsNullOrEmpty(user.CompanyId))
        {
            claims.Add(new Claim("CompanyId", user.CompanyId));
        }

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

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
    {

        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            throw new AppException("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            throw new AppException("Refresh token is already revoked or expired");
        }

        var user = storedToken.User;
        if (user == null)
        {
            throw new AppException("User associated with the refresh token not found");
        }

        // Generate new tokens
        var newTokenResponse = await GenerateTokenWithRefreshAsync(user);

        // Revoke the old refresh token
        await _refreshTokenRepository.RevokeTokenAsync(refreshToken, user.Id, "Replaced by new token");

        return newTokenResponse;

    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? revokedBy = null, string? reason = null)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            throw new AppException("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            throw new AppException("Refresh token is already revoked or expired");
        }

        await _refreshTokenRepository.RevokeTokenAsync(refreshToken, revokedBy, reason ?? "Token revoked by user");

        return true;

    }

    public async Task<bool> RevokeAllUserTokensAsync(string userId, string? revokedBy = null, string? reason = null)
    {
        await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, revokedBy, reason ?? "All tokens revoked");
        return true;


    }

    public async Task<bool> IsRefreshTokenValidAsync(string refreshToken)
    {
        return await _refreshTokenRepository.IsTokenActiveAsync(refreshToken);
    }
}