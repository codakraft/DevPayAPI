using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<Company> CreateCompany(Company company);
    Task<List<Company>> GetAllCompanies();
    Task<Company?> GetCompaniesById(Guid id);
    Task<Company> UpdateCompany(Company company);
}