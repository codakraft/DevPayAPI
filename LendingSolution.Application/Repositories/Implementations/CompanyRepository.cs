using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _db;

    public CompanyRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Company> CreateCompany(Company company)
    {
        await _db.Companies.AddAsync(company);
        await _db.SaveChangesAsync();
        return company;
    }

    public async Task<List<Company>> GetAllCompanies()
    {
        return await _db.Companies.ToListAsync();
    }

    public async Task<Company?> GetCompaniesById(Guid id)   
    {
        return await _db.Companies.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Company> UpdateCompany(Company company)
    {
        _db.Companies.Update(company);
        await _db.SaveChangesAsync();
        return company;
    }
}