using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;

namespace LendingSolution.Application.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<ApiResponse> CreateCompany(CreateCompanyRequestDto body)
    {
        var company = new Company
        {
            Name = body.Name,
            Street = body.Street,
            ShortName = body.ShortName,
            State = body.State,
            City = body.City

        };
        var dbResponse = await _companyRepository.CreateCompany(company);

        if (company == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Company was not created"
            };
        }

        return new ApiResponse
        {
            Success = true,
            Data = new CreateCompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                Street = company.Street,
                ShortName = company.ShortName,
                State = company.State,
                City = company.City
            }
        };
    }



}