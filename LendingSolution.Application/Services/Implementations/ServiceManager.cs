using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Application.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace LendingSolution.Application.Services.Implementations;

public class ServiceManager(
#pragma warning disable CS9113 // Parameter is unread.
    IHttpContextAccessor _contextAccessor,
#pragma warning restore CS9113 // Parameter is unread.
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<JwtSettings> jwtconfig,
    ITokenService tokenService,
    ApplicationDbContext db,
    ICompanyRepository companyRepository,
    ILoanRepository loanRepository,
    ILoanProductRepository loanProductRepository,
    IRemitaService remitaService,
    IConfiguration configuration,
    IOptions<RemitaSettings> remitaOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<RemitaService> logger,
    ICombinedRepository cRepo,
    IRefreshTokenRepository refreshTokenRepository,
    IDisbursementRepository disbursementRepository,
    IRepaymentRepository repaymentRepository,
    ISupportTicketRepository supportTicketRepository
) : IServiceManager
{
    private readonly Lazy<IAuthService> _authService = new Lazy<IAuthService>(() =>
                new AuthService(
                    userManager,
                    tokenService,
                    db,
                    companyRepository,
                    loanRepository,
                    remitaService,
                    roleManager,
                    disbursementRepository,
                    repaymentRepository,
                    supportTicketRepository
                ));
    private readonly Lazy<ITokenService> _tokenService = new Lazy<ITokenService>(() =>
                new TokenService(userManager, jwtconfig, refreshTokenRepository));
    private readonly Lazy<ICompanyService> _companyService = new Lazy<ICompanyService>(() =>
                new CompanyService(companyRepository, userManager, loanRepository, disbursementRepository, repaymentRepository));
    private readonly Lazy<ILoanService> _loanService = new Lazy<ILoanService>(() =>
                new LoanService(
                    db,
                    userManager,
                    remitaService,
                    loanRepository,
                    companyRepository,
                    configuration
                ));
    private readonly Lazy<IRemitaService> _remitaService = new Lazy<IRemitaService>(() =>
                new RemitaService(
                    remitaOptions,
                    httpClientFactory,
                    logger,
                    cRepo
                ));
    private readonly Lazy<ILoanProductService> _loanProductService = new Lazy<ILoanProductService>(() =>
                new LoanProductService(loanProductRepository, companyRepository));
    private readonly Lazy<ISupportToolsService> _supportToolsService = new Lazy<ISupportToolsService>(() =>
                new SupportToolsService());

    public IAuthService AuthService => _authService.Value;
    public ITokenService TokenService => _tokenService.Value;
    public ICompanyService CompanyService => _companyService.Value;
    public ILoanService LoanService => _loanService.Value;
    public IRemitaService RemitaService => _remitaService.Value;
    public ILoanProductService LoanProductService => _loanProductService.Value;
    public ISupportToolsService SupportToolsService => _supportToolsService.Value;
}
