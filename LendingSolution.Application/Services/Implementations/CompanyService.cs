using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Enum;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoanRepository _loanRepository;
    private readonly IDisbursementRepository _disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository;
    private readonly IWalletService _walletService;

    public CompanyService(
        ICompanyRepository companyRepository,
        UserManager<ApplicationUser> userManager,
        ILoanRepository loanRepository,
        IDisbursementRepository disbursementRepository,
        IRepaymentRepository repaymentRepository,
        IWalletService walletService)
    {
        _companyRepository = companyRepository;
        _userManager = userManager;
        _loanRepository = loanRepository;
        _disbursementRepository = disbursementRepository;
        _repaymentRepository = repaymentRepository;
        _walletService = walletService;
    }

    public async Task Activate(Guid id)
    {
        var company = await _companyRepository.GetCompanyById(id);

        if (company is null)
        {
            throw new AppException("Company does not exist", 404);
        }

        if (company.IsActive is true)
        {
            throw new AppException("Company is already active", 400);
        }

        company.IsActive = true;

        var updateCompany = await _companyRepository.UpdateCompany(company);

        if (updateCompany is false)
        {
            throw new AppException("Unable to activate company at the moment", 500);
        }
    }

    public async Task<CompanyResponseDto> CreateCompany(CreateCompanyRequestDto body)
    {
        var company = new Company
        {
            Name = body.Name,
            Street = body.Street,
            ShortName = body.ShortName,
            State = body.State,
            City = body.City,
            LogoDocumentId = body.LogoDocumentId

        };
        var dbResponse = await _companyRepository.CreateCompany(company);

        if (company == null)
        {
            throw new AppException("Company was not created", 500);
        }

        // Automatically create a wallet for the new company with starting balance of 0
        try
        {
            await _walletService.CreateCompanyWalletAsync(company.Id);
        }
        catch (AppException)
        {
            // If wallet already exists, ignore the error - this shouldn't happen but we handle it gracefully
            // Log this if needed but don't fail the company creation
        }

        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            Street = company.Street,
            ShortName = company.ShortName,
            State = company.State,
            City = company.City,
            LogoDocumentId = company.LogoDocumentId
        };
    }

    public async Task Deactivate(Guid id)
    {
        var company = await _companyRepository.GetCompanyById(id);

        if (company is null)
        {
            throw new AppException("Company does not exist", 404);
        }

        if (company.IsActive is false)
        {
            throw new AppException("Company is already inactive", 400);
        }

        company.IsActive = false;

        var updateCompany = await _companyRepository.UpdateCompany(company);

        if (updateCompany is false)
        {
            throw new AppException("Unable to deactivate company at the moment", 500);
        }
    }

    public async Task<CompanyResponseDto> UpdateCompany(Guid id, UpdateCompanyRequestDto body, string? userId = null)
    {
        var company = await _companyRepository.GetCompanyById(id);

        if (company is null)
        {
            throw new AppException("Company does not exist", 404);
        }

        // Update company properties
        company.Name = body.Name;
        company.ShortName = body.ShortName;
        company.Street = body.Street;
        company.City = body.City;
        company.State = body.State;
        company.LogoDocumentId = body.LogoDocumentId;
        company.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _companyRepository.UpdateCompany(company);

        if (!updateResult)
        {
            throw new AppException("Unable to update company at the moment", 500);
        }

        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            ShortName = company.ShortName,
            Street = company.Street,
            City = company.City,
            State = company.State,
            LogoDocumentId = company.LogoDocumentId
        };
    }

    public async Task<CompanyResponseDto> GetCompanyById(Guid id)
    {
        var company = await _companyRepository.GetCompanyById(id);

        if (company is null)
        {
            throw new AppException("Company not found", 404);
        }

        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            ShortName = company.ShortName,
            Street = company.Street,
            City = company.City,
            State = company.State,
            LogoDocumentId = company.LogoDocumentId
        };
    }

    public async Task<CompanyResponseDto> GetUserCompany(string userId)
    {
        var company = await _companyRepository.GetCompanyByUserId(userId);

        if (company is null)
        {
            throw new AppException("User is not associated with any company", 404);
        }

        return new CompanyResponseDto
        {
            Id = company.Id,
            Name = company.Name,
            ShortName = company.ShortName,
            Street = company.Street,
            City = company.City,
            State = company.State,
            LogoDocumentId = company.LogoDocumentId
        };
    }

    public async Task<PagedCompanyListDto> GetAllCompaniesAsync(CompanyFilterDto filter)
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
                LogoDocumentId = company.LogoDocumentId,
                LogoUrl = null, // We can populate this when we have document service integration
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

        return new PagedCompanyListDto
        {
            Companies = paginatedCompanies,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };
    }

    public async Task<CompanyListDto> GetCompanyByIdAsync(Guid id)
    {
        var company = await _companyRepository.GetCompanyById(id);
        if (company == null)
        {
            throw new AppException("Company not found", 404);
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

        return new CompanyListDto
        {
            Id = company.Id,
            Name = company.Name,
            ShortName = company.ShortName,
            Email = null, // Company model doesn't have Email
            PhoneNumber = null, // Company model doesn't have PhoneNumber
            Address = $"{company.Street}, {company.City}, {company.State}".Trim(' ', ','),
            IsActive = company.IsActive,
            LogoDocumentId = company.LogoDocumentId,
            LogoUrl = null, // We can populate this when we have document service integration
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
    }

    public async Task<PagedCompanyUserListDto> GetCompanyUsersAsync(Guid companyId, CompanyUserFilterDto filter)
    {
        // Verify company exists
        var company = await _companyRepository.GetCompanyById(companyId);
        if (company == null)
        {
            throw new AppException("Company not found", 404);
        }

        // Get company users
        var users = await _userManager.Users
            .Where(u => u.CompanyId == companyId.ToString())
            .ToListAsync();

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            users = users.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm))).ToList();
        }

        // Apply filters
        if (filter.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == filter.IsActive.Value).ToList();
        }

        if (!string.IsNullOrEmpty(filter.Gender))
        {
            users = users.Where(u => u.Gender == filter.Gender).ToList();
        }

        if (filter.CreatedFrom.HasValue)
        {
            users = users.Where(u => u.CreatedAt >= filter.CreatedFrom.Value).ToList();
        }

        if (filter.CreatedTo.HasValue)
        {
            users = users.Where(u => u.CreatedAt <= filter.CreatedTo.Value.AddDays(1)).ToList();
        }

        if (filter.LastLoginFrom.HasValue)
        {
            users = users.Where(u => u.LastLoginAt >= filter.LastLoginFrom.Value).ToList();
        }

        if (filter.LastLoginTo.HasValue)
        {
            users = users.Where(u => u.LastLoginAt <= filter.LastLoginTo.Value.AddDays(1)).ToList();
        }

        // Map users to DTOs with roles
        var userDtos = new List<CompanyUserDto>();
        foreach (var user in users)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            
            var userDto = new CompanyUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Role = userRoles.FirstOrDefault()
            };

            userDtos.Add(userDto);
        }

        var totalCount = userDtos.Count;

        // Apply role-based filtering
        if (!string.IsNullOrEmpty(filter.Role))
        {
            userDtos = userDtos.Where(u => u.Role == filter.Role).ToList();
            totalCount = userDtos.Count;
        }

        // Apply sorting
        userDtos = filter.SortBy?.ToLower() switch
        {
            "firstname" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.FirstName).ToList()
                : userDtos.OrderBy(u => u.FirstName).ToList(),
            "lastname" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.LastName).ToList()
                : userDtos.OrderBy(u => u.LastName).ToList(),
            "email" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.Email).ToList()
                : userDtos.OrderBy(u => u.Email).ToList(),
            "isactive" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.IsActive).ToList()
                : userDtos.OrderBy(u => u.IsActive).ToList(),
            "role" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.Role ?? string.Empty).ToList()
                : userDtos.OrderBy(u => u.Role ?? string.Empty).ToList(),
            "lastloginat" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.LastLoginAt).ToList()
                : userDtos.OrderBy(u => u.LastLoginAt).ToList(),
            "createdat" => filter.SortOrder?.ToLower() == "desc"
                ? userDtos.OrderByDescending(u => u.CreatedAt).ToList()
                : userDtos.OrderBy(u => u.CreatedAt).ToList(),
            _ => userDtos.OrderByDescending(u => u.CreatedAt).ToList()
        };

        // Apply pagination
        var paginatedUsers = userDtos
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        // Calculate summary statistics
        var totalActiveUsers = userDtos.Count(u => u.IsActive);
        var totalInactiveUsers = userDtos.Count(u => !u.IsActive);

        return new PagedCompanyUserListDto
        {
            Users = paginatedUsers,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1,
            TotalActiveUsers = totalActiveUsers,
            TotalInactiveUsers = totalInactiveUsers
        };
    }

}