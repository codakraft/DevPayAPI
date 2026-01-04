using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class RemitaSalaryHistoryRepository : IRemitaSalaryHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public RemitaSalaryHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RemitaSalaryHistory?> GetByBorrowerApplicationIdAsync(Guid borrowerApplicationId)
    {
        return await _context.RemitaSalaryHistories
            .Include(rsh => rsh.SalaryPayments)
            .Include(rsh => rsh.BorrowerApplication)
            .FirstOrDefaultAsync(rsh => rsh.BorrowerApplicationId == borrowerApplicationId);
    }

    public async Task<RemitaSalaryHistory> CreateAsync(RemitaSalaryHistory salaryHistory)
    {
        _context.RemitaSalaryHistories.Add(salaryHistory);
        await _context.SaveChangesAsync();
        return salaryHistory;
    }

    public async Task UpdateAsync(RemitaSalaryHistory salaryHistory)
    {
        salaryHistory.UpdatedAt = DateTime.UtcNow;
        _context.RemitaSalaryHistories.Update(salaryHistory);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var salaryHistory = await _context.RemitaSalaryHistories.FindAsync(id);
        if (salaryHistory != null)
        {
            _context.RemitaSalaryHistories.Remove(salaryHistory);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsForBorrowerApplicationAsync(Guid borrowerApplicationId)
    {
        return await _context.RemitaSalaryHistories
            .AnyAsync(rsh => rsh.BorrowerApplicationId == borrowerApplicationId);
    }

    public async Task<RemitaSalaryHistory?> GetSalaryHistoryByBvnAsync(string bvn)
    {
        return await _context.RemitaSalaryHistories
            .Include(rsh => rsh.SalaryPayments)
            .Include(rsh => rsh.BorrowerApplication)
                .ThenInclude(ba => ba!.Company)
            .FirstOrDefaultAsync(rsh => rsh.BVN == bvn);
    }

    public async Task<PaginatedSalaryHistoryResponseDto> GetSalaryHistoryByCompanyAsync(Guid companyId, SalaryHistoryFilterRequestDto filters)
    {
        var baseQuery = _context.RemitaSalaryHistories
            .Include(rsh => rsh.SalaryPayments)
            .Include(rsh => rsh.BorrowerApplication)
                .ThenInclude(ba => ba!.Company)
            .Where(rsh => rsh.BorrowerApplication!.CompanyId == companyId);

        var filteredQuery = ApplyFilters(baseQuery, filters);
        
        return await GetPaginatedResultAsync(filteredQuery, filters);
    }

    public async Task<PaginatedSalaryHistoryResponseDto> GetAllSalaryHistoryAsync(SalaryHistoryFilterRequestDto filters)
    {
        var baseQuery = _context.RemitaSalaryHistories
            .Include(rsh => rsh.SalaryPayments)
            .Include(rsh => rsh.BorrowerApplication)
                .ThenInclude(ba => ba!.Company);

        var filteredQuery = ApplyFilters(baseQuery, filters);
        
        return await GetPaginatedResultAsync(filteredQuery, filters);
    }

    public async Task<SalaryHistoryViewDto?> GetSalaryHistoryDetailsAsync(Guid salaryHistoryId)
    {
        var salaryHistory = await _context.RemitaSalaryHistories
            .Include(rsh => rsh.SalaryPayments)
            .Include(rsh => rsh.BorrowerApplication)
                .ThenInclude(ba => ba!.Company)
            .FirstOrDefaultAsync(rsh => rsh.Id == salaryHistoryId);

        if (salaryHistory == null)
            return null;

        return MapToViewDto(salaryHistory);
    }

    private static IQueryable<RemitaSalaryHistory> ApplyFilters(IQueryable<RemitaSalaryHistory> query, SalaryHistoryFilterRequestDto filters)
    {
        if (!string.IsNullOrEmpty(filters.BorrowerEmail))
        {
            query = query.Where(rsh => rsh.BorrowerApplication!.Email.Contains(filters.BorrowerEmail));
        }

        if (!string.IsNullOrEmpty(filters.BVN))
        {
            query = query.Where(rsh => rsh.BVN!.Contains(filters.BVN));
        }

        if (!string.IsNullOrEmpty(filters.EmployerName))
        {
            query = query.Where(rsh => rsh.CompanyName!.Contains(filters.EmployerName));
        }

        if (filters.CompanyId.HasValue)
        {
            query = query.Where(rsh => rsh.BorrowerApplication!.CompanyId == filters.CompanyId.Value);
        }

        if (filters.FromDate.HasValue)
        {
            query = query.Where(rsh => rsh.CreatedAt >= filters.FromDate.Value);
        }

        if (filters.ToDate.HasValue)
        {
            query = query.Where(rsh => rsh.CreatedAt <= filters.ToDate.Value);
        }

        if (filters.MinSalaryAmount.HasValue)
        {
            query = query.Where(rsh => rsh.AverageMonthlySalary >= filters.MinSalaryAmount.Value);
        }

        if (filters.MaxSalaryAmount.HasValue)
        {
            query = query.Where(rsh => rsh.AverageMonthlySalary <= filters.MaxSalaryAmount.Value);
        }

        if (filters.HasOutstandingLoans.HasValue)
        {
            query = query.Where(rsh => rsh.HasOutstandingLoans == filters.HasOutstandingLoans.Value);
        }

        // Apply sorting
        query = filters.SortBy.ToLower() switch
        {
            "createdat" => filters.SortOrder.ToLower() == "asc" 
                ? query.OrderBy(rsh => rsh.CreatedAt)
                : query.OrderByDescending(rsh => rsh.CreatedAt),
            "averagesalary" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(rsh => rsh.AverageMonthlySalary)
                : query.OrderByDescending(rsh => rsh.AverageMonthlySalary),
            "employername" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(rsh => rsh.CompanyName)
                : query.OrderByDescending(rsh => rsh.CompanyName),
            "borroweremail" => filters.SortOrder.ToLower() == "asc"
                ? query.OrderBy(rsh => rsh.BorrowerApplication!.Email)
                : query.OrderByDescending(rsh => rsh.BorrowerApplication!.Email),
            _ => query.OrderByDescending(rsh => rsh.CreatedAt)
        };

        return query;
    }

    private static async Task<PaginatedSalaryHistoryResponseDto> GetPaginatedResultAsync(IQueryable<RemitaSalaryHistory> query, SalaryHistoryFilterRequestDto filters)
    {
        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / filters.PageSize);

        var salaryHistories = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        var data = salaryHistories.Select(MapToViewDto).ToList();

        return new PaginatedSalaryHistoryResponseDto
        {
            Data = data,
            TotalRecords = totalRecords,
            CurrentPage = filters.Page,
            PageSize = filters.PageSize,
            TotalPages = totalPages,
            HasNextPage = filters.Page < totalPages,
            HasPreviousPage = filters.Page > 1
        };
    }

    private static SalaryHistoryViewDto MapToViewDto(RemitaSalaryHistory salaryHistory)
    {
        return new SalaryHistoryViewDto
        {
            Id = salaryHistory.Id,
            BorrowerApplicationId = salaryHistory.BorrowerApplicationId,
            BorrowerEmail = salaryHistory.BorrowerApplication?.Email ?? "Unknown",
            BorrowerFullName = $"{salaryHistory.BorrowerApplication?.FirstName ?? ""} {salaryHistory.BorrowerApplication?.LastName ?? ""}".Trim(),
            CompanyName = salaryHistory.BorrowerApplication?.Company?.Name ?? "Unknown Company",
            BVN = salaryHistory.BVN ?? "Unknown",
            AccountNumber = salaryHistory.AccountNumber,
            BankCode = salaryHistory.BankCode,
            BankName = "Unknown Bank", // This would need to be mapped from bank code
            EmployerName = salaryHistory.CompanyName ?? "Unknown Employer",
            TotalSalaryReceived = salaryHistory.SalaryPayments.Sum(sp => sp.Amount),
            PaymentCount = salaryHistory.SalaryCount,
            FirstPaymentDate = salaryHistory.FirstPaymentDate ?? DateTime.MinValue,
            LastPaymentDate = salaryHistory.LatestPaymentDate ?? DateTime.MinValue,
            AverageMonthlySalary = salaryHistory.AverageMonthlySalary,
            LastSalaryAmount = salaryHistory.LatestSalaryAmount,
            HasOutstandingLoans = false,
            TotalOutstandingAmount = 0,
            OutstandingLoanCount = 0,
            CreatedAt = salaryHistory.CreatedAt,
            SalaryPayments = salaryHistory.SalaryPayments.Select(sp => new SalaryPaymentViewDto
            {
                Amount = sp.Amount,
                PaymentDate = sp.PaymentDate,
                PaymentReference = sp.AccountNumber, // Using account number as reference
                Narration = $"Salary payment to {sp.AccountNumber}"
            }).OrderByDescending(sp => sp.PaymentDate).ToList(),
            OutstandingLoans = new List<RemitaLoanViewDto>()
        };
    }
}
