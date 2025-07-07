using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ILoanProductRepository
{
    Task<bool> CreateLoanProduct(LoanProduct loanProduct);
    Task<LoanProduct?> GetLoanProductById(Guid id);
    Task<bool> UpdateLoanProduct(LoanProduct loanProduct);
    Task<List<LoanProduct>> GetLoanProductsByCompanyId(Guid companyId);
    IQueryable<LoanProduct> GetAllLoanProductsQueryable(); // For filtering and pagination
    IQueryable<LoanProduct> GetCompanyLoanProductsQueryable(Guid companyId); // For company-specific filtering
    // Task<bool> DeleteLoanProduct(Guid id);
}