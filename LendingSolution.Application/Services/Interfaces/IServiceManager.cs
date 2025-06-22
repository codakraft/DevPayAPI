namespace LendingSolution.Application.Services.Interfaces;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    ITokenService TokenService { get; }
    ICompanyService CompanyService { get; }
    ILoanService LoanService { get; }
    IRemitaService RemitaService { get; }
    ILoanProductService LoanProductService { get; }
    ISupportToolsService SupportToolsService { get; }

}


