using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ILoanProductService
    {
        Task<IEnumerable<LoanProductDto>> GetLoanProductsAsync(LoanProductFilterDto filter);
        Task<LoanProductDto> GetLoanProductByIdAsync(string id);
        Task<bool> DeactivateLoanProductAsync(string id);
        Task<bool> ActivateLoanProductAsync(string id);
        Task<bool> DeleteLoanProductAsync(string id);
        Task<bool> UpdateLoanProductAsync(string id, LoanProductUpdateDto update);
    }

}
