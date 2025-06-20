using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ISuperAdminService
    {
        Task<Company> OnboardCompanyAsync(string name, string branding = null);
        Task<bool> ActivateCompanyAsync(Guid companyId);
        Task<bool> DeactivateCompanyAsync(Guid companyId);
        Task<IEnumerable<Company>> GetAllCompaniesAsync();
        Task<Company> GetCompanyByIdAsync(Guid companyId);
    }
}
