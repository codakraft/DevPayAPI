using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ILoanProductService
    {
        Task<ApiResponse> CreateLoanProduct(CreateLoanProductRequestDto product);
        Task<List<LoanProductResponseDto>> GetLoanProductsByCompanyId(Guid companyId);
        // Task<bool> DeactivateLoanProductAsync(string id);
        // Task<bool> ActivateLoanProductAsync(string id);
        // Task<bool> DeleteLoanProductAsync(string id);
        // Task<bool> UpdateLoanProductAsync(string id, LoanProductUpdateDto update);
    }

}
