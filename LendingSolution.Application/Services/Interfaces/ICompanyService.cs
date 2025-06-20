using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface ICompanyService
{
   Task<ApiResponse<CreateCompanyResponseDto>> CreateCompany(CreateCompanyRequestDto body);
}