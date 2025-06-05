using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using LendingSolution.Infrastructure.Data;


namespace LendingSolution.Application.Services.Implementations
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthService> _authService;
        private readonly Lazy<ITokenService> _tokenService;
        private readonly Lazy<IProfileService> _profileService;
        public ServiceManager(IHttpContextAccessor _contextAccessor,
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> configuration, ITokenService tokenService, ApplicationDbContext db)
        {
            _authService = new Lazy<IAuthService>(() =>
                    new AuthService(userManager, tokenService, db));
            _profileService = new Lazy<IProfileService>(() =>
                    new ProfileService(userManager, _contextAccessor));
            _tokenService = new Lazy<ITokenService>(() =>
                    new TokenService(userManager, configuration));
        }

        public IAuthService AuthService => _authService.Value;
        public ITokenService TokenService => _tokenService.Value;
        public IProfileService ProfileService => _profileService.Value;
    }
}