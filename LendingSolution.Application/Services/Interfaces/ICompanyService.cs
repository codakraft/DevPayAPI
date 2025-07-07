using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface ICompanyService
{
   Task<ApiResponse> CreateCompany(CreateCompanyRequestDto body);
   Task<ApiResponse> UpdateCompany(Guid id, UpdateCompanyRequestDto body, string? userId = null);
   Task<ApiResponse> GetCompanyById(Guid id);
   Task<ApiResponse> GetCompanyByIdAsync(Guid id); // Add async version for detailed company info
   Task<ApiResponse> GetUserCompany(string userId);
   Task<ApiResponse> Activate(Guid id);
   Task<ApiResponse> Deactivate(Guid id);
   Task<ApiResponse> GetAllCompaniesAsync(CompanyFilterDto filter);
}