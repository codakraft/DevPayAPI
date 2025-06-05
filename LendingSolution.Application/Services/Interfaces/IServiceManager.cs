namespace LendingSolution.Application.Services.Interfaces;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    ITokenService TokenService { get; }
}


