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

    public async Task<ApiResponse> UpdateLoanProduct(Guid id, UpdateLoanProductRequestDto dto, string? userId = null)
    {
        try
        {
            var existingProduct = await _loanProductRepository.GetLoanProductById(id);

            if (existingProduct == null)
            {
                return ApiResponse.Fail("Loan product not found");
            }

            // Update the loan product properties
            existingProduct.Name = dto.Name;
            existingProduct.ShortName = dto.ShortName;
            existingProduct.Description = dto.Description;
            existingProduct.InterestRate = dto.InterestRate;
            existingProduct.MinAmount = dto.MinAmount;
            existingProduct.MaxAmount = dto.MaxAmount;
            existingProduct.MinTenor = dto.MinTenor;
            existingProduct.MaxTenor = dto.MaxTenor;
            existingProduct.Moratorium = dto.Moratorium;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _loanProductRepository.UpdateLoanProduct(existingProduct);

            if (!updateResult)
            {
                return ApiResponse.Fail("Failed to update loan product");
            }

            return ApiResponse.Ok("Loan product updated successfully", new LoanProductResponseDto
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                ShortName = existingProduct.ShortName,
                Description = existingProduct.Description,
                InterestRate = existingProduct.InterestRate,
                MinAmount = existingProduct.MinAmount,
                MaxAmount = existingProduct.MaxAmount,
                MinTenor = existingProduct.MinTenor,
                MaxTenor = existingProduct.MaxTenor,
                Moratorium = existingProduct.Moratorium,
                CompanyId = existingProduct.CompanyId
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to update loan product: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetLoanProductById(Guid id)
    {
        try
        {
            var loanProduct = await _loanProductRepository.GetLoanProductById(id);

            if (loanProduct == null)
            {
                return ApiResponse.Fail("Loan product not found");
            }

            return ApiResponse.Ok("Loan product retrieved successfully", new LoanProductResponseDto
            {
                Id = loanProduct.Id,
                Name = loanProduct.Name,
                ShortName = loanProduct.ShortName,
                Description = loanProduct.Description,
                InterestRate = loanProduct.InterestRate,
                MinAmount = loanProduct.MinAmount,
                MaxAmount = loanProduct.MaxAmount,
                MinTenor = loanProduct.MinTenor,
                MaxTenor = loanProduct.MaxTenor,
                Moratorium = loanProduct.Moratorium,
                CompanyId = loanProduct.CompanyId
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve loan product: {ex.Message}");
        }
    }

    private async Task<bool> CompanyExistsAsync(Guid companyId)
        => await _companyRepository.GetCompanyById(companyId) is not null;
}