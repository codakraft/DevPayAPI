using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations;

public class LoanProductService(
    ILoanProductRepository loanProductRepository,
    ICompanyRepository companyRepository
) : ILoanProductService
{
    private readonly ILoanProductRepository _loanProductRepository = loanProductRepository;
    private readonly ICompanyRepository _companyRepository = companyRepository;

    public async Task<LoanProductResponseDto> CreateLoanProduct(CreateLoanProductRequestDto dto, Guid companyId)
    {
        if (!await CompanyExistsAsync(companyId))
            throw new AppException("Company does not exist", 404);

        var loanProduct = new LoanProduct
        {
            CompanyId = companyId,
            Name = dto.Name,
            Code = dto.Code,
            ShortName = dto.ShortName,
            Description = dto.Description,
            MinAmount = dto.MinAmount,
            MaxAmount = dto.MaxAmount,
            MinTenor = dto.MinTenor,
            MaxTenor = dto.MaxTenor,
            InterestRate = dto.InterestRate,
            PenaltyOnDefaultPrincipal = dto.PenaltyOnDefaultPrincipal,
            Moratorium = dto.Moratorium,
            NotifyApprovalsViaEmail = dto.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = dto.TurnoverEligibilityPercent,
            InterestComputationBasis = dto.InterestComputationBasis,
            InterestCostComputation = dto.InterestCostComputation,
            PaymentScheduleBreakdown = dto.PaymentScheduleBreakdown,
            PaymentScheduleType = dto.PaymentScheduleType
        };

        var created = await _loanProductRepository.CreateLoanProduct(loanProduct);

        if (!created)
        {
            throw new AppException("Failed to create loan product", 400);
        }

        return new LoanProductResponseDto
        {
            Id = loanProduct.Id,
            CompanyId = loanProduct.CompanyId,
            Name = loanProduct.Name,
            Code = loanProduct.Code,
            ShortName = loanProduct.ShortName,
            Description = loanProduct.Description,
            MinAmount = loanProduct.MinAmount,
            MaxAmount = loanProduct.MaxAmount,
            MinTenor = loanProduct.MinTenor,
            MaxTenor = loanProduct.MaxTenor,
            InterestRate = loanProduct.InterestRate,
            PenaltyOnDefaultPrincipal = loanProduct.PenaltyOnDefaultPrincipal,
            Moratorium = loanProduct.Moratorium,
            NotifyApprovalsViaEmail = loanProduct.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = loanProduct.TurnoverEligibilityPercent,
            InterestComputationBasis = loanProduct.InterestComputationBasis,
            InterestCostComputation = loanProduct.InterestCostComputation,
            PaymentScheduleBreakdown = loanProduct.PaymentScheduleBreakdown,
            PaymentScheduleType = loanProduct.PaymentScheduleType,
            IsActive = loanProduct.IsActive,
            CreatedAt = loanProduct.CreatedAt,
            UpdatedAt = loanProduct.UpdatedAt
        };

    }

    public async Task<List<LoanProductResponseDto>> GetLoanProductsByCompanyId(Guid companyId)
    {
        if (!await CompanyExistsAsync(companyId))
            throw new AppException("Company does not exist", 404);

        var loanProducts = await _loanProductRepository.GetLoanProductsByCompanyId(companyId);

        var data = loanProducts.Select(lp => new LoanProductResponseDto
        {
            Id = lp.Id,
            CompanyId = lp.CompanyId,
            Name = lp.Name,
            Code = lp.Code,
            ShortName = lp.ShortName,
            Description = lp.Description,
            MinAmount = lp.MinAmount,
            MaxAmount = lp.MaxAmount,
            MinTenor = lp.MinTenor,
            MaxTenor = lp.MaxTenor,
            InterestRate = lp.InterestRate,
            PenaltyOnDefaultPrincipal = lp.PenaltyOnDefaultPrincipal,
            Moratorium = lp.Moratorium,
            NotifyApprovalsViaEmail = lp.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = lp.TurnoverEligibilityPercent,
            InterestComputationBasis = lp.InterestComputationBasis,
            InterestCostComputation = lp.InterestCostComputation,
            PaymentScheduleBreakdown = lp.PaymentScheduleBreakdown,
            PaymentScheduleType = lp.PaymentScheduleType,
            IsActive = lp.IsActive,
            CreatedAt = lp.CreatedAt,
            UpdatedAt = lp.UpdatedAt
        }).ToList();

        return data;
    }

    public async Task<LoanProductResponseDto> UpdateLoanProduct(Guid id, UpdateLoanProductRequestDto dto, string? userId = null)
    {
        var existingProduct = await _loanProductRepository.GetLoanProductById(id);

        if (existingProduct == null)
        {
            throw new AppException("Loan product not found", 404);
        }

        // Update the loan product properties
        existingProduct.Name = dto.Name;
        existingProduct.Code = dto.Code;
        existingProduct.ShortName = dto.ShortName;
        existingProduct.Description = dto.Description;
        existingProduct.MinAmount = dto.MinAmount;
        existingProduct.MaxAmount = dto.MaxAmount;
        existingProduct.MinTenor = dto.MinTenor;
        existingProduct.MaxTenor = dto.MaxTenor;
        existingProduct.InterestRate = dto.InterestRate;
        existingProduct.PenaltyOnDefaultPrincipal = dto.PenaltyOnDefaultPrincipal;
        existingProduct.Moratorium = dto.Moratorium;
        existingProduct.NotifyApprovalsViaEmail = dto.NotifyApprovalsViaEmail;
        existingProduct.TurnoverEligibilityPercent = dto.TurnoverEligibilityPercent;
        existingProduct.InterestComputationBasis = dto.InterestComputationBasis;
        existingProduct.InterestCostComputation = dto.InterestCostComputation;
        existingProduct.PaymentScheduleBreakdown = dto.PaymentScheduleBreakdown;
        existingProduct.PaymentScheduleType = dto.PaymentScheduleType;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _loanProductRepository.UpdateLoanProduct(existingProduct);

        if (!updateResult)
        {
            throw new AppException("Failed to update loan product", 400);
        }

        return new LoanProductResponseDto
        {
            Id = existingProduct.Id,
            CompanyId = existingProduct.CompanyId,
            Name = existingProduct.Name,
            Code = existingProduct.Code,
            ShortName = existingProduct.ShortName,
            Description = existingProduct.Description,
            MinAmount = existingProduct.MinAmount,
            MaxAmount = existingProduct.MaxAmount,
            MinTenor = existingProduct.MinTenor,
            MaxTenor = existingProduct.MaxTenor,
            InterestRate = existingProduct.InterestRate,
            PenaltyOnDefaultPrincipal = existingProduct.PenaltyOnDefaultPrincipal,
            Moratorium = existingProduct.Moratorium,
            NotifyApprovalsViaEmail = existingProduct.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = existingProduct.TurnoverEligibilityPercent,
            InterestComputationBasis = existingProduct.InterestComputationBasis,
            InterestCostComputation = existingProduct.InterestCostComputation,
            PaymentScheduleBreakdown = existingProduct.PaymentScheduleBreakdown,
            PaymentScheduleType = existingProduct.PaymentScheduleType,
            IsActive = existingProduct.IsActive,
            CreatedAt = existingProduct.CreatedAt,
            UpdatedAt = existingProduct.UpdatedAt
        };

    }

    public async Task<LoanProductResponseDto> GetLoanProductById(Guid id)
    {
        var loanProduct = await _loanProductRepository.GetLoanProductById(id);

        if (loanProduct == null)
        {
            throw new AppException("Loan product not found", 404);
        }

        return new LoanProductResponseDto
        {
            Id = loanProduct.Id,
            CompanyId = loanProduct.CompanyId,
            Name = loanProduct.Name,
            Code = loanProduct.Code,
            ShortName = loanProduct.ShortName,
            Description = loanProduct.Description,
            MinAmount = loanProduct.MinAmount,
            MaxAmount = loanProduct.MaxAmount,
            MinTenor = loanProduct.MinTenor,
            MaxTenor = loanProduct.MaxTenor,
            InterestRate = loanProduct.InterestRate,
            PenaltyOnDefaultPrincipal = loanProduct.PenaltyOnDefaultPrincipal,
            Moratorium = loanProduct.Moratorium,
            NotifyApprovalsViaEmail = loanProduct.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = loanProduct.TurnoverEligibilityPercent,
            InterestComputationBasis = loanProduct.InterestComputationBasis,
            InterestCostComputation = loanProduct.InterestCostComputation,
            PaymentScheduleBreakdown = loanProduct.PaymentScheduleBreakdown,
            PaymentScheduleType = loanProduct.PaymentScheduleType,
            IsActive = loanProduct.IsActive,
            CreatedAt = loanProduct.CreatedAt,
            UpdatedAt = loanProduct.UpdatedAt
        };
    }
    public async Task<PagedLoanProductListDto> GetAllLoanProductsAsync(LoanProductFilterDto filter)
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
            Code = lp.Code,
            Description = lp.Description,
            ShortName = lp.ShortName,
            MinAmount = lp.MinAmount,
            MaxAmount = lp.MaxAmount,
            MinTenor = lp.MinTenor,
            MaxTenor = lp.MaxTenor,
            InterestRate = lp.InterestRate,
            PenaltyOnDefaultPrincipal = lp.PenaltyOnDefaultPrincipal,
            Moratorium = lp.Moratorium,
            NotifyApprovalsViaEmail = lp.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = lp.TurnoverEligibilityPercent,
            InterestComputationBasis = lp.InterestComputationBasis,
            InterestCostComputation = lp.InterestCostComputation,
            PaymentScheduleBreakdown = lp.PaymentScheduleBreakdown,
            PaymentScheduleType = lp.PaymentScheduleType,
            IsActive = lp.IsActive,
            CreatedAt = lp.CreatedAt,
            UpdatedAt = lp.UpdatedAt,
            CompanyId = lp.CompanyId,
            CompanyName = lp.Company.Name,
            CompanyShortName = lp.Company.ShortName,
            CompanyIsActive = lp.Company.IsActive
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedLoanProductListDto
        {
            LoanProducts = loanProductDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };

    }

    public async Task<PagedLoanProductListDto> GetCompanyLoanProductsAsync(Guid companyId, LoanProductFilterDto filter)
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
            Code = lp.Code,
            Description = lp.Description,
            ShortName = lp.ShortName,
            MinAmount = lp.MinAmount,
            MaxAmount = lp.MaxAmount,
            MinTenor = lp.MinTenor,
            MaxTenor = lp.MaxTenor,
            InterestRate = lp.InterestRate,
            PenaltyOnDefaultPrincipal = lp.PenaltyOnDefaultPrincipal,
            Moratorium = lp.Moratorium,
            NotifyApprovalsViaEmail = lp.NotifyApprovalsViaEmail,
            TurnoverEligibilityPercent = lp.TurnoverEligibilityPercent,
            InterestComputationBasis = lp.InterestComputationBasis,
            InterestCostComputation = lp.InterestCostComputation,
            PaymentScheduleBreakdown = lp.PaymentScheduleBreakdown,
            PaymentScheduleType = lp.PaymentScheduleType,
            IsActive = lp.IsActive,
            CreatedAt = lp.CreatedAt,
            UpdatedAt = lp.UpdatedAt,
            CompanyId = lp.CompanyId,
            CompanyName = lp.Company.Name,
            CompanyShortName = lp.Company.ShortName,
            CompanyIsActive = lp.Company.IsActive
        }).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedLoanProductListDto
        {
            LoanProducts = loanProductDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };

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

        // Enum filters
        if (filter.InterestComputationBasis.HasValue)
        {
            query = query.Where(lp => lp.InterestComputationBasis == filter.InterestComputationBasis.Value);
        }
        if (filter.InterestCostComputation.HasValue)
        {
            query = query.Where(lp => lp.InterestCostComputation == filter.InterestCostComputation.Value);
        }
        if (filter.PaymentScheduleBreakdown.HasValue)
        {
            query = query.Where(lp => lp.PaymentScheduleBreakdown == filter.PaymentScheduleBreakdown.Value);
        }
        if (filter.PaymentScheduleType.HasValue)
        {
            query = query.Where(lp => lp.PaymentScheduleType == filter.PaymentScheduleType.Value);
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