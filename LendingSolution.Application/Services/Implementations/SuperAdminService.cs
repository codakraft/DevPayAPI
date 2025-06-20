using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class SuperAdminService : ISuperAdminService
    {
        private readonly ApplicationDbContext _db;
        public SuperAdminService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Company> OnboardCompanyAsync(string name, string branding = null)
        {
            var company = new Company
            {
                Id = Guid.NewGuid(),
                Name = name,
                Status = "Active",
                Branding = branding,
                CreatedAt = DateTime.UtcNow
            };
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();
            return company;
        }

        public async Task<bool> ActivateCompanyAsync(Guid companyId)
        {
            var company = await _db.Companies.FindAsync(companyId);
            if (company == null || company.Status == "Active") return false;
            company.Status = "Active";
            _db.Companies.Update(company);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateCompanyAsync(Guid companyId)
        {
            var company = await _db.Companies.FindAsync(companyId);
            if (company == null || company.Status == "Inactive") return false;
            company.Status = "Inactive";
            _db.Companies.Update(company);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            return await _db.Companies.ToListAsync();
        }

        public async Task<Company> GetCompanyByIdAsync(Guid companyId)
        {
            return await _db.Companies.FindAsync(companyId);
        }
    }
}
