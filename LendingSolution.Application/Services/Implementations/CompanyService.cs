using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Enum;

namespace LendingSolution.Application.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoanRepository _loanRepository;
    private readonly IDisbursementRepository _disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository;

    public CompanyService(
        ICompanyRepository companyRepository,
        UserManager<ApplicationUser> userManager,
        ILoanRepository loanRepository,
        IDisbursementRepository disbursementRepository,
        IRepaymentRepository repaymentRepository)
    {
        _companyRepository = companyRepository;
        _userManager = userManager;
        _loanRepository = loanRepository;
        _disbursementRepository = disbursementRepository;
        _repaymentRepository = repaymentRepository;
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

    public async Task<ApiResponse> UpdateCompany(Guid id, UpdateCompanyRequestDto body, string? userId = null)
    {
        try
        {
            var company = await _companyRepository.GetCompanyById(id);

            if (company is null)
            {
                return ApiResponse.Fail("Company does not exist");
            }

            // Update company properties
            company.Name = body.Name;
            company.ShortName = body.ShortName;
            company.Street = body.Street;
            company.City = body.City;
            company.State = body.State;
            company.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _companyRepository.UpdateCompany(company);

            if (!updateResult)
            {
                return ApiResponse.Fail("Unable to update company at the moment");
            }

            return ApiResponse.Ok("Company updated successfully", new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                ShortName = company.ShortName,
                Street = company.Street,
                City = company.City,
                State = company.State
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to update company: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetCompanyById(Guid id)
    {
        try
        {
            var company = await _companyRepository.GetCompanyById(id);

            if (company is null)
            {
                return ApiResponse.Fail("Company not found");
            }

            return ApiResponse.Ok("Company retrieved successfully", new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                ShortName = company.ShortName,
                Street = company.Street,
                City = company.City,
                State = company.State
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve company: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetUserCompany(string userId)
    {
        try
        {
            var company = await _companyRepository.GetCompanyByUserId(userId);

            if (company is null)
            {
                return ApiResponse.Fail("User is not associated with any company");
            }

            return ApiResponse.Ok("Company retrieved successfully", new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                ShortName = company.ShortName,
                Street = company.Street,
                City = company.City,
                State = company.State
            });
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve user company: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetAllCompaniesAsync(CompanyFilterDto filter)
    {
        try
        {
            var allCompanies = await _companyRepository.GetAllCompanies();
            var companyList = allCompanies.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchTerm = filter.Search.ToLower();
                companyList = companyList.Where(c => 
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.ShortName.ToLower().Contains(searchTerm) ||
                    (c.Street != null && c.Street.ToLower().Contains(searchTerm)) ||
                    (c.City != null && c.City.ToLower().Contains(searchTerm)) ||
                    (c.State != null && c.State.ToLower().Contains(searchTerm)));
            }

            // Apply active status filter
            if (filter.IsActive.HasValue)
            {
                companyList = companyList.Where(c => c.IsActive == filter.IsActive.Value);
            }

            // Apply date range filters
            if (filter.CreatedFrom.HasValue)
            {
                companyList = companyList.Where(c => c.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                companyList = companyList.Where(c => c.CreatedAt <= filter.CreatedTo.Value.AddDays(1));
            }

            // Calculate metrics for each company
            var companyDtos = new List<CompanyListDto>();
            
            foreach (var company in companyList)
            {
                // Get company users
                var users = await _userManager.Users
                    .Where(u => u.CompanyId == company.Id.ToString())
                    .ToListAsync();
                
                var activeUsers = users.Count(u => u.IsActive);
                
                // Get company loans using the correct method
                var companyIdGuid = company.Id;
                var loans = await _loanRepository.GetAllLoansByCompanyId(companyIdGuid);
                var loansList = loans.ToList();
                
                var activeLoans = loansList.Count(l => l.Status == LoanStatus.Disbursed);
                var pendingLoans = loansList.Count(l => l.Status == LoanStatus.Pending);
                
                // Get financial metrics - for now using sample calculations
                // Note: We need to implement GetByCompanyIdAsync methods in repositories
                var totalDisbursed = loansList.Where(l => l.Status == LoanStatus.Disbursed).Sum(l => l.Amount);
                var outstandingAmount = totalDisbursed; // Simplified for now
                
                var overdueLoans = loansList.Count(l => l.Status == LoanStatus.Overdue);
                var defaultRate = loansList.Any() ? (double)overdueLoans / loansList.Count() * 100 : 0;

                // Get last activity (most recent loan, user registration, or company update)
                var lastLoanDate = loansList.Any() ? loansList.Max(l => l.CreatedAt) : (DateTime?)null;
                var lastUserDate = users.Any() ? users.Max(u => u.CreatedAt) : (DateTime?)null;
                var lastActivity = new[] { lastLoanDate, lastUserDate, company.UpdatedAt }
                    .Where(d => d.HasValue)
                    .Max();

                var companyDto = new CompanyListDto
                {
                    Id = company.Id,
                    Name = company.Name,
                    ShortName = company.ShortName,
                    Email = null, // Company model doesn't have Email
                    PhoneNumber = null, // Company model doesn't have PhoneNumber
                    Address = $"{company.Street}, {company.City}, {company.State}".Trim(' ', ','),
                    IsActive = company.IsActive,
                    CreatedAt = company.CreatedAt,
                    UpdatedAt = company.UpdatedAt,
                    TotalUsers = users.Count,
                    ActiveUsers = activeUsers,
                    TotalLoans = loansList.Count(),
                    ActiveLoans = activeLoans,
                    PendingLoans = pendingLoans,
                    TotalLoanAmount = loansList.Sum(l => l.Amount),
                    TotalDisbursed = totalDisbursed,
                    OutstandingAmount = outstandingAmount,
                    DefaultRate = Math.Round(defaultRate, 2),
                    LastActivity = lastActivity
                };

                companyDtos.Add(companyDto);
            }

            // Apply numeric filters after calculating metrics
            if (filter.MinUsers.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.TotalUsers >= filter.MinUsers.Value).ToList();
            }

            if (filter.MaxUsers.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.TotalUsers <= filter.MaxUsers.Value).ToList();
            }

            if (filter.MinLoans.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.TotalLoans >= filter.MinLoans.Value).ToList();
            }

            if (filter.MaxLoans.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.TotalLoans <= filter.MaxLoans.Value).ToList();
            }

            if (filter.MinLoanAmount.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.TotalLoanAmount >= filter.MinLoanAmount.Value).ToList();
            }

            if (filter.MaxLoanAmount.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.TotalLoanAmount <= filter.MaxLoanAmount.Value).ToList();
            }

            if (filter.MinDefaultRate.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.DefaultRate >= filter.MinDefaultRate.Value).ToList();
            }

            if (filter.MaxDefaultRate.HasValue)
            {
                companyDtos = companyDtos.Where(c => c.DefaultRate <= filter.MaxDefaultRate.Value).ToList();
            }

            var totalCount = companyDtos.Count;

            // Apply sorting
            companyDtos = filter.SortBy?.ToLower() switch
            {
                "name" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.Name).ToList()
                    : companyDtos.OrderBy(c => c.Name).ToList(),
                "shortname" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.ShortName).ToList()
                    : companyDtos.OrderBy(c => c.ShortName).ToList(),
                "isactive" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.IsActive).ToList()
                    : companyDtos.OrderBy(c => c.IsActive).ToList(),
                "totalusers" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.TotalUsers).ToList()
                    : companyDtos.OrderBy(c => c.TotalUsers).ToList(),
                "totalloans" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.TotalLoans).ToList()
                    : companyDtos.OrderBy(c => c.TotalLoans).ToList(),
                "totalloanamount" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.TotalLoanAmount).ToList()
                    : companyDtos.OrderBy(c => c.TotalLoanAmount).ToList(),
                "defaultrate" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.DefaultRate).ToList()
                    : companyDtos.OrderBy(c => c.DefaultRate).ToList(),
                "lastactivity" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.LastActivity).ToList()
                    : companyDtos.OrderBy(c => c.LastActivity).ToList(),
                "createdat" => filter.SortOrder?.ToLower() == "desc"
                    ? companyDtos.OrderByDescending(c => c.CreatedAt).ToList()
                    : companyDtos.OrderBy(c => c.CreatedAt).ToList(),
                _ => companyDtos.OrderByDescending(c => c.CreatedAt).ToList()
            };

            // Apply pagination
            var paginatedCompanies = companyDtos
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var result = new PagedCompanyListDto
            {
                Companies = paginatedCompanies,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalPages = totalPages,
                HasNextPage = filter.Page < totalPages,
                HasPreviousPage = filter.Page > 1
            };

            return ApiResponse.Ok("Companies retrieved successfully", result);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error retrieving companies: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetCompanyByIdAsync(Guid id)
    {
        try
        {
            var company = await _companyRepository.GetCompanyById(id);
            if (company == null)
            {
                return ApiResponse.Fail("Company not found");
            }

            // Get company users
            var users = await _userManager.Users
                .Where(u => u.CompanyId == id.ToString())
                .ToListAsync();
            
            var activeUsers = users.Count(u => u.IsActive);
            
            // Get company loans
            var loans = await _loanRepository.GetAllLoansByCompanyId(id);
            var loansList = loans.ToList();
            
            var activeLoans = loansList.Count(l => l.Status == LoanStatus.Disbursed);
            var pendingLoans = loansList.Count(l => l.Status == LoanStatus.Pending);
            var overdueLoans = loansList.Count(l => l.Status == LoanStatus.Overdue);
            
            // Get financial metrics
            var totalDisbursed = loansList.Where(l => l.Status == LoanStatus.Disbursed).Sum(l => l.Amount);
            var outstandingAmount = totalDisbursed; // Simplified for now
            var defaultRate = loansList.Any() ? (double)overdueLoans / loansList.Count() * 100 : 0;

            // Get last activity
            var lastLoanDate = loansList.Any() ? loansList.Max(l => l.CreatedAt) : (DateTime?)null;
            var lastUserDate = users.Any() ? users.Max(u => u.CreatedAt) : (DateTime?)null;
            var lastActivity = new[] { lastLoanDate, lastUserDate, company.UpdatedAt }
                .Where(d => d.HasValue)
                .Max();

            var companyDetails = new CompanyListDto
            {
                Id = company.Id,
                Name = company.Name,
                ShortName = company.ShortName,
                Email = null, // Company model doesn't have Email
                PhoneNumber = null, // Company model doesn't have PhoneNumber
                Address = $"{company.Street}, {company.City}, {company.State}".Trim(' ', ','),
                IsActive = company.IsActive,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                TotalUsers = users.Count,
                ActiveUsers = activeUsers,
                TotalLoans = loansList.Count(),
                ActiveLoans = activeLoans,
                PendingLoans = pendingLoans,
                TotalLoanAmount = loansList.Sum(l => l.Amount),
                TotalDisbursed = totalDisbursed,
                OutstandingAmount = outstandingAmount,
                DefaultRate = Math.Round(defaultRate, 2),
                LastActivity = lastActivity
            };

            return ApiResponse.Ok("Company retrieved successfully", companyDetails);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Error retrieving company: {ex.Message}");
        }
    }

}