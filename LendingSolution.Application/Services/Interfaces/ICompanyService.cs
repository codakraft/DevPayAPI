using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface ICompanyService
{
   Task<CompanyResponseDto> CreateCompany(CreateCompanyRequestDto body);
   Task<CompanyResponseDto> UpdateCompany(Guid id, UpdateCompanyRequestDto body, string? userId = null);
   Task<CompanyResponseDto> GetCompanyById(Guid id);
   Task<CompanyListDto> GetCompanyByIdAsync(Guid id); // Add async version for detailed company info
   Task<CompanyResponseDto> GetUserCompany(string userId);
   Task Activate(Guid id);
   Task Deactivate(Guid id);
   Task<PagedCompanyListDto> GetAllCompaniesAsync(CompanyFilterDto filter);
}