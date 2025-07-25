using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ILoanProductService
    {
        Task<LoanProductResponseDto> CreateLoanProduct(CreateLoanProductRequestDto product, Guid companyId);
        Task<LoanProductResponseDto> UpdateLoanProduct(Guid id, UpdateLoanProductRequestDto product, string? userId = null);
        Task<LoanProductResponseDto> GetLoanProductById(Guid id);
        Task<List<LoanProductResponseDto>> GetLoanProductsByCompanyId(Guid companyId);

        // New methods for filtering and search
        Task<PagedLoanProductListDto> GetAllLoanProductsAsync(LoanProductFilterDto filter); // For SuperAdmin
        Task<PagedLoanProductListDto> GetCompanyLoanProductsAsync(Guid companyId, LoanProductFilterDto filter); // For Admin

        // ...existing code...
    }

}
