using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Service interface for salary history viewing operations
/// </summary>
public interface ISalaryHistoryViewService
{
    /// <summary>
    /// Get salary history for a specific company (Admin access)
    /// </summary>
    Task<PaginatedSalaryHistoryResponseDto> GetCompanySalaryHistoryAsync(Guid companyId, SalaryHistoryFilterRequestDto filters);

    /// <summary>
    /// Get salary history for all companies (SuperAdmin access)
    /// </summary>
    Task<PaginatedSalaryHistoryResponseDto> GetAllSalaryHistoryAsync(SalaryHistoryFilterRequestDto filters);

    /// <summary>
    /// Get detailed salary history by ID
    /// </summary>
    Task<SalaryHistoryViewDto> GetSalaryHistoryDetailsAsync(Guid salaryHistoryId);

    /// <summary>
    /// Validate company access to salary history record
    /// </summary>
    Task<bool> ValidateCompanyAccessAsync(Guid companyId, Guid salaryHistoryId);
}
