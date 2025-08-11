using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IRemitaSalaryHistoryRepository
{
    Task<RemitaSalaryHistory?> GetByBorrowerApplicationIdAsync(Guid borrowerApplicationId);
    Task<RemitaSalaryHistory> CreateAsync(RemitaSalaryHistory salaryHistory);
    Task UpdateAsync(RemitaSalaryHistory salaryHistory);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsForBorrowerApplicationAsync(Guid borrowerApplicationId);

    /// <summary>
    /// Get salary history by BVN
    /// </summary>
    Task<RemitaSalaryHistory?> GetSalaryHistoryByBvnAsync(string bvn);

    /// <summary>
    /// Get paginated salary history for a specific company (Admin access)
    /// </summary>
    Task<PaginatedSalaryHistoryResponseDto> GetSalaryHistoryByCompanyAsync(Guid companyId, SalaryHistoryFilterRequestDto filters);

    /// <summary>
    /// Get paginated salary history for all companies (SuperAdmin access)
    /// </summary>
    Task<PaginatedSalaryHistoryResponseDto> GetAllSalaryHistoryAsync(SalaryHistoryFilterRequestDto filters);

    /// <summary>
    /// Get salary history details by ID
    /// </summary>
    Task<SalaryHistoryViewDto?> GetSalaryHistoryDetailsAsync(Guid salaryHistoryId);
}
