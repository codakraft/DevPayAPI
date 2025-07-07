using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<Company> CreateCompany(Company company);
    Task<IEnumerable<Company>> GetAllCompanies();
    Task<Company?> GetCompanyById(Guid id);
    Task<Company?> GetCompanyByUserId(string userId);
    Task<bool> UpdateCompany(Company company);
}