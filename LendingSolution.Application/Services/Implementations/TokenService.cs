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
using LendingSolution.Core.Auth;

namespace LendingSolution.Application.Services.Implementations;

public class TokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> configuration,
    IRefreshTokenRepository refreshTokenRepository,
    RoleManager<IdentityRole> roleManager) : ITokenService
{
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
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
                new("sstamp", user.SecurityStamp ?? string.Empty),
            };

        // Add CompanyId claim for Admin users
        if (!string.IsNullOrEmpty(user.CompanyId))
        {
            claims.Add(new Claim("CompanyId", user.CompanyId));
        }

        // Restricts the token to change-password/logout/refresh until the password is changed
        if (user.RequiresPasswordChange)
        {
            claims.Add(new Claim("pwd_change_required", "true"));
        }

        var roles = await _userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(await GetPermissionClaimsAsync(roles));
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

    /// <summary>
    /// Collects the distinct permission claims granted by the user's roles.
    /// </summary>
    private async Task<IEnumerable<Claim>> GetPermissionClaimsAsync(IEnumerable<string> roleNames)
    {
        var permissions = new HashSet<string>();
        foreach (var roleName in roleNames)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is null) continue;

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            permissions.UnionWith(roleClaims.Where(c => c.Type == Permissions.ClaimType).Select(c => c.Value));
        }

        return permissions.Select(p => new Claim(Permissions.ClaimType, p));
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

        if (!user.IsActive)
        {
            await _refreshTokenRepository.RevokeTokenAsync(refreshToken, user.Id, "User deactivated");
            throw new AppException("Account is deactivated. Please contact your administrator.", 403, ErrorCodes.AccountDeactivated);
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