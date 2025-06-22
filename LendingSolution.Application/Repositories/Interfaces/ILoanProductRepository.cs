using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ILoanProductRepository
{
    Task<bool> CreateLoanProduct(LoanProduct loanProduct);
    Task<List<LoanProduct>> GetLoanProductsByCompanyId(Guid companyId);
    // Task<List<LoanProduct>> GetAllLoanProducts();
    // Task<LoanProduct?> GetLoanProductById(Guid id);
    // Task<LoanProduct> UpdateLoanProduct(LoanProduct loanProduct);
    // Task<bool> DeleteLoanProduct(Guid id);
}