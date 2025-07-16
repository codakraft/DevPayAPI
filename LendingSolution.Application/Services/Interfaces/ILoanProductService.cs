using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ILoanProductService
    {
        Task<ApiResponse> CreateLoanProduct(CreateLoanProductRequestDto product, Guid companyId);
        Task<ApiResponse> UpdateLoanProduct(Guid id, UpdateLoanProductRequestDto product, string? userId = null);
        Task<ApiResponse> GetLoanProductById(Guid id);
        Task<List<LoanProductResponseDto>> GetLoanProductsByCompanyId(Guid companyId);
        
        // New methods for filtering and search
        Task<ApiResponse> GetAllLoanProductsAsync(LoanProductFilterDto filter); // For SuperAdmin
        Task<ApiResponse> GetCompanyLoanProductsAsync(Guid companyId, LoanProductFilterDto filter); // For Admin
        
        // ...existing code...
    }

}
