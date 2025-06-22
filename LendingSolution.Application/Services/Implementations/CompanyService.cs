using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<ApiResponse> Activate(Guid id)
    {
        var company = await _companyRepository.GetCompanyById(id);

        if (company is null)
        {
            return ApiResponse.Fail("Company does not exist");
        }

        if (company.IsActive is true)
        {
            return ApiResponse.Fail("Company is already active");
        }

        company.IsActive = true;

        var updateCompany = await _companyRepository.UpdateCompany(company);

        if (updateCompany is false)
        {
            return ApiResponse.Fail("Unable to activate company at the moment");
        }

        return ApiResponse.Ok("Company has been activated");
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

        return ApiResponse.Ok(
            "Company created successfully",
            new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                Street = company.Street,
                ShortName = company.ShortName,
                State = company.State,
                City = company.City
            });
    }

    public async Task<ApiResponse> Deactivate(Guid id)
    {
        var company = await _companyRepository.GetCompanyById(id);

        if (company is null)
        {
            return ApiResponse.Fail("Company does not exist");
        }

        if (company.IsActive is false)
        {
            return ApiResponse.Fail("Company is already inactive");
        }

        company.IsActive = false;

        var updateCompany = await _companyRepository.UpdateCompany(company);

        if (updateCompany is false)
        {
            return ApiResponse.Fail("Unable to deactivate company at the moment");
        }

        return ApiResponse.Ok("Company has been deactivated");
    }
}