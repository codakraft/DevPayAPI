using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IBorrowerApplicationRepository
{
    Task<BorrowerApplication> CreateAsync(BorrowerApplication application);
    Task<BorrowerApplication?> GetByIdAsync(Guid id);
    Task<BorrowerApplication?> GetByEmailAsync(string email);
    Task<BorrowerApplication?> GetByLoanIdAsync(Guid loanId);
    Task<bool> UpdateAsync(BorrowerApplication application);
    Task<bool> DeleteAsync(Guid id);
    Task<List<BorrowerApplication>> GetByCompanyIdAsync(Guid companyId);
    Task<List<BorrowerApplication>> GetByStepAsync(BorrowerOnboardingStep step);
    Task<bool> ExistsByEmailAsync(string email);
}
