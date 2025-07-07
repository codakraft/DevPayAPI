using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ApiResponse> GetAllLoanProductsAsync(LoanProductFilterDto filter)
    {
        try
        {
            var query = _loanProductRepository.GetAllLoanProductsQueryable();

            // Apply filters
            query = ApplyFilters(query, filter);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var pagedLoanProducts = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Map to DTOs
            var loanProductDtos = pagedLoanProducts.Select(lp => new LoanProductListDto
            {
                Id = lp.Id,
                Name = lp.Name,
                Description = lp.Description,
                ShortName = lp.ShortName,
                InterestRate = lp.InterestRate,
                MinAmount = lp.MinAmount,
                MaxAmount = lp.MaxAmount,
                MinTenor = lp.MinTenor,
                MaxTenor = lp.MaxTenor,
                IsActive = lp.IsActive,
                Moratorium = lp.Moratorium,
                CreatedAt = lp.CreatedAt,
                UpdatedAt = lp.UpdatedAt,
                CompanyId = lp.CompanyId,
                CompanyName = lp.Company.Name,
                CompanyShortName = lp.Company.ShortName,
                CompanyIsActive = lp.Company.IsActive
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var result = new PagedLoanProductListDto
            {
                LoanProducts = loanProductDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };

            return ApiResponse.Ok("Loan products retrieved successfully", result);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error retrieving loan products: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetCompanyLoanProductsAsync(Guid companyId, LoanProductFilterDto filter)
    {
        try
        {
            var query = _loanProductRepository.GetCompanyLoanProductsQueryable(companyId);

            // Apply filters (excluding company filter since it's already filtered)
            query = ApplyFilters(query, filter, excludeCompanyFilter: true);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter);

            // Apply pagination
            var pagedLoanProducts = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            // Map to DTOs
            var loanProductDtos = pagedLoanProducts.Select(lp => new LoanProductListDto
            {
                Id = lp.Id,
                Name = lp.Name,
                Description = lp.Description,
                ShortName = lp.ShortName,
                InterestRate = lp.InterestRate,
                MinAmount = lp.MinAmount,
                MaxAmount = lp.MaxAmount,
                MinTenor = lp.MinTenor,
                MaxTenor = lp.MaxTenor,
                IsActive = lp.IsActive,
                Moratorium = lp.Moratorium,
                CreatedAt = lp.CreatedAt,
                UpdatedAt = lp.UpdatedAt,
                CompanyId = lp.CompanyId,
                CompanyName = lp.Company.Name,
                CompanyShortName = lp.Company.ShortName,
                CompanyIsActive = lp.Company.IsActive
            }).ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var result = new PagedLoanProductListDto
            {
                LoanProducts = loanProductDtos,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };

            return ApiResponse.Ok("Company loan products retrieved successfully", result);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error retrieving company loan products: {ex.Message}");
        }
    }

    private IQueryable<LoanProduct> ApplyFilters(IQueryable<LoanProduct> query, LoanProductFilterDto filter, bool excludeCompanyFilter = false)
    {
        // Search filter (name, description, short name)
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(lp =>
                lp.Name.ToLower().Contains(searchTerm) ||
                lp.Description.ToLower().Contains(searchTerm) ||
                lp.ShortName.ToLower().Contains(searchTerm) ||
                lp.Company.Name.ToLower().Contains(searchTerm));
        }

        // Company filter (only for SuperAdmin view)
        if (!excludeCompanyFilter && filter.CompanyId.HasValue)
        {
            query = query.Where(lp => lp.CompanyId == filter.CompanyId.Value);
        }

        // Active status filter
        if (filter.IsActive.HasValue)
        {
            query = query.Where(lp => lp.IsActive == filter.IsActive.Value);
        }

        // Interest rate range filter
        if (filter.MinInterestRate.HasValue)
        {
            query = query.Where(lp => lp.InterestRate >= filter.MinInterestRate.Value);
        }
        if (filter.MaxInterestRate.HasValue)
        {
            query = query.Where(lp => lp.InterestRate <= filter.MaxInterestRate.Value);
        }

        // Amount range filter
        if (filter.MinAmount.HasValue)
        {
            query = query.Where(lp => lp.MaxAmount >= filter.MinAmount.Value);
        }
        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(lp => lp.MinAmount <= filter.MaxAmount.Value);
        }

        // Tenor range filter
        if (filter.MinTenor.HasValue)
        {
            query = query.Where(lp => lp.MaxTenor >= filter.MinTenor.Value);
        }
        if (filter.MaxTenor.HasValue)
        {
            query = query.Where(lp => lp.MinTenor <= filter.MaxTenor.Value);
        }

        return query;
    }

    private IQueryable<LoanProduct> ApplySorting(IQueryable<LoanProduct> query, LoanProductFilterDto filter)
    {
        return filter.SortBy?.ToLower() switch
        {
            "name" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.Name)
                : query.OrderBy(lp => lp.Name),
            "interestrate" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.InterestRate)
                : query.OrderBy(lp => lp.InterestRate),
            "minamount" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.MinAmount)
                : query.OrderBy(lp => lp.MinAmount),
            "maxamount" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.MaxAmount)
                : query.OrderBy(lp => lp.MaxAmount),
            "companyname" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.Company.Name)
                : query.OrderBy(lp => lp.Company.Name),
            "updatedat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.UpdatedAt)
                : query.OrderBy(lp => lp.UpdatedAt),
            "createdat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(lp => lp.CreatedAt)
                : query.OrderBy(lp => lp.CreatedAt),
            _ => query.OrderByDescending(lp => lp.CreatedAt)
        };
    }

    private async Task<bool> CompanyExistsAsync(Guid companyId)
        => await _companyRepository.GetCompanyById(companyId) is not null;
}