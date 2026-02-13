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
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IRemitaService remitaService,
    IConfiguration configuration,
    IOptions<RemitaSettings> remitaOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<RemitaService> logger,
    ILogger<LoanService> loanServiceLogger,
    ICombinedRepository cRepo,
    IRefreshTokenRepository refreshTokenRepository,
    IDisbursementRepository disbursementRepository,
    IRepaymentRepository repaymentRepository,
    ISupportTicketRepository supportTicketRepository,
    IBorrowerApplicationRepository borrowerApplicationRepository,
    IRemitaSalaryHistoryRepository remitaSalaryHistoryRepository,
    IWalletService walletService,
    IEmailService emailService,
    IDocumentService documentService,
    IProvidusDisbursementService providusDisbursementService,
    IAuditService auditService,
    IOtpService otpService,
    IMfaSessionRepository mfaSessionRepository
) : IServiceManager
{
    private readonly Lazy<IAuthService> _authService = new Lazy<IAuthService>(() =>
                new AuthService(
                    userManager,
                    tokenService,
                    companyRepository,
                    loanRepository,
                    employeeRepository,
                    userRepository,
                    remitaService,
                    roleManager,
                    disbursementRepository,
                    repaymentRepository,
                    supportTicketRepository,
                    auditService,
                    otpService,
                    mfaSessionRepository
                ));
    private readonly Lazy<ITokenService> _tokenService = new Lazy<ITokenService>(() =>
                new TokenService(userManager, jwtconfig, refreshTokenRepository));
    private readonly Lazy<ICompanyService> _companyService = new Lazy<ICompanyService>(() =>
                new CompanyService(companyRepository, userManager, loanRepository, disbursementRepository, repaymentRepository, walletService));
    private readonly Lazy<ILoanService> _loanService = new Lazy<ILoanService>(() =>
                new LoanService(
                    userManager,
                    remitaService,
                    loanRepository,
                    companyRepository,
                    borrowerApplicationRepository,
                    remitaSalaryHistoryRepository,
                    configuration,
                    emailService,
                    documentService,
                    providusDisbursementService,
                    db,
                    loanServiceLogger,
                    auditService
                ));
    private readonly Lazy<IRemitaService> _remitaService = new Lazy<IRemitaService>(() =>
                new RemitaService(
                    remitaOptions,
                    httpClientFactory,
                    logger,
                    cRepo,
                    db
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
