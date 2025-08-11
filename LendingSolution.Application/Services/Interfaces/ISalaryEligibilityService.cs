using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces;

public interface ISalaryEligibilityService
{
    /// <summary>
    /// Calculate loan eligibility based on salary history and product constraints
    /// </summary>
    Task<SalaryEligibilityDto> CalculateLoanEligibilityAsync(RemitaSalaryHistoryResponseDto salaryData, LoanProduct product);

    /// <summary>
    /// Save salary history data to database
    /// </summary>
    Task<RemitaSalaryHistory> SaveSalaryHistoryAsync(Guid borrowerApplicationId, RemitaSalaryHistoryResponseDto salaryData);

    /// <summary>
    /// Parse Remita date string to DateTime
    /// </summary>
    DateTime? ParseRemitaDate(string? dateString);
}
