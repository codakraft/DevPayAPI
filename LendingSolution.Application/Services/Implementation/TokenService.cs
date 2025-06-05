using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LendingSolution.Application.Services.Implementations;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;

    public TokenService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> configuration)
    {
        _userManager = userManager;
        _jwtSettings = configuration.Value;
    }


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
                // new("Role", user.Role)
            };


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
}