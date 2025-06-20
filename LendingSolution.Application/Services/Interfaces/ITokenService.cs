using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces;

public interface ITokenService
{

    Task<string> GenerateTokenAsync(ApplicationUser user);

}