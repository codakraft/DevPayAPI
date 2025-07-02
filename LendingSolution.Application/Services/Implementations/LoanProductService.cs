using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Implementations;

public class LoanProductService(
    ILoanProductRepository loanProductRepository,
    ICompanyRepository companyRepository
) : ILoanProductService
{
    private readonly ILoanProductRepository _loanProductRepository = loanProductRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<ApiResponse> CreateLoanProduct(CreateLoanProductRequestDto dto)
    {
        if (!await CompanyExistsAsync(dto.CompanyId))
            return ApiResponse.Fail("Company does not exist");

        var loanProduct = new LoanProduct
        {
            Name = dto.Name,
            InterestRate = dto.InterestRate,
            MinAmount = dto.MinAmount,
            MaxAmount = dto.MaxAmount,
            Description = dto.Description,
            CompanyId = dto.CompanyId,
            ShortName = dto.ShortName
        };

        var created = await _loanProductRepository.CreateLoanProduct(loanProduct);

        return created
            ? ApiResponse.Ok("Loan product created successfully")
            : ApiResponse.Fail("Failed to create loan product");
    }

    public async Task<List<LoanProductResponseDto>> GetLoanProductsByCompanyId(Guid companyId)
    {
        if (!await CompanyExistsAsync(companyId))
            throw new AppException("Company does not exist", 404);

        var loanProducts = await _loanProductRepository.GetLoanProductsByCompanyId(companyId);

        var data = loanProducts.Select(lp => new LoanProductResponseDto
        {
            Id = lp.Id,
            Name = lp.Name,
            InterestRate = lp.InterestRate,
            MinAmount = lp.MinAmount,
            MaxAmount = lp.MaxAmount,
            Description = lp.Description,
            CompanyId = lp.CompanyId,
            ShortName = lp.ShortName
        }).ToList();

        return data;
    }

    private async Task<bool> CompanyExistsAsync(Guid companyId)
        => await _companyRepository.GetCompanyById(companyId) is not null;
}