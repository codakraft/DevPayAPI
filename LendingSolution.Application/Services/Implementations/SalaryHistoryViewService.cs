using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Service for salary history viewing operations
/// </summary>
public class SalaryHistoryViewService : ISalaryHistoryViewService
{
    private readonly IRemitaSalaryHistoryRepository _salaryHistoryRepository;
    private readonly ILogger<SalaryHistoryViewService> _logger;

    public SalaryHistoryViewService(
        IRemitaSalaryHistoryRepository salaryHistoryRepository,
        ILogger<SalaryHistoryViewService> logger)
    {
        _salaryHistoryRepository = salaryHistoryRepository;
        _logger = logger;
    }

    public async Task<PaginatedSalaryHistoryResponseDto> GetCompanySalaryHistoryAsync(Guid companyId, SalaryHistoryFilterRequestDto filters)
    {
        try
        {
            _logger.LogInformation("Fetching salary history for company {CompanyId} with filters", companyId);
            
            var result = await _salaryHistoryRepository.GetSalaryHistoryByCompanyAsync(companyId, filters);
            
            _logger.LogInformation("Retrieved {Count} salary history records for company {CompanyId}", 
                result.Data.Count, companyId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching salary history for company {CompanyId}", companyId);
            throw new AppException("An error occurred while retrieving salary history", 500);
        }
    }

    public async Task<PaginatedSalaryHistoryResponseDto> GetAllSalaryHistoryAsync(SalaryHistoryFilterRequestDto filters)
    {
        try
        {
            _logger.LogInformation("Fetching all salary history records with filters");
            
            var result = await _salaryHistoryRepository.GetAllSalaryHistoryAsync(filters);
            
            _logger.LogInformation("Retrieved {Count} salary history records across all companies", 
                result.Data.Count);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all salary history records");
            throw new AppException("An error occurred while retrieving salary history", 500);
        }
    }

    public async Task<SalaryHistoryViewDto> GetSalaryHistoryDetailsAsync(Guid salaryHistoryId)
    {
        try
        {
            _logger.LogInformation("Fetching salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            
            var salaryHistory = await _salaryHistoryRepository.GetSalaryHistoryDetailsAsync(salaryHistoryId);
            
            if (salaryHistory == null)
            {
                throw new AppException("Salary history not found", 404);
            }
            
            _logger.LogInformation("Retrieved salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            
            return salaryHistory;
        }
        catch (AppException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching salary history details for ID {SalaryHistoryId}", salaryHistoryId);
            throw new AppException("An error occurred while retrieving salary history details", 500);
        }
    }

    public async Task<bool> ValidateCompanyAccessAsync(Guid companyId, Guid salaryHistoryId)
    {
        try
        {
            _logger.LogInformation("Validating company {CompanyId} access to salary history {SalaryHistoryId}", 
                companyId, salaryHistoryId);
            
            var salaryHistory = await _salaryHistoryRepository.GetSalaryHistoryDetailsAsync(salaryHistoryId);
            
            if (salaryHistory == null)
            {
                return false;
            }
            
            // Check through borrower application to see if it belongs to the company
            var hasAccess = salaryHistory.BorrowerApplicationId != Guid.Empty;
            
            if (hasAccess)
            {
                // Additional validation can be added here if needed
                // For now, we assume the repository already filters by company
                var companySpecificResult = await _salaryHistoryRepository.GetSalaryHistoryByCompanyAsync(
                    companyId, 
                    new SalaryHistoryFilterRequestDto { Page = 1, PageSize = 1 });
                
                hasAccess = companySpecificResult.Data.Any(sh => sh.Id == salaryHistoryId);
            }
            
            _logger.LogInformation("Company {CompanyId} access to salary history {SalaryHistoryId}: {HasAccess}", 
                companyId, salaryHistoryId, hasAccess);
            
            return hasAccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating company access for salary history {SalaryHistoryId}", salaryHistoryId);
            return false;
        }
    }
}
