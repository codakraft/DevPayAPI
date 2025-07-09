using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Enum;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;
namespace LendingSolution.Application.Services.Implementations;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ICompanyRepository companyRepository,
    ILoanRepository loanRepository,
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IRemitaService remitaService,
    RoleManager<IdentityRole> roleManager,
    IDisbursementRepository disbursementRepository,
    IRepaymentRepository repaymentRepository,
    ISupportTicketRepository supportTicketRepository
) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly IDisbursementRepository _disbursementRepository = disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository = repaymentRepository;
    private readonly ISupportTicketRepository _supportTicketRepository = supportTicketRepository;

    private static bool VerifyBvn(String Bvn)
    {
        // if (!Bvn.Contains("2222222"))
        // {
        //     return false;
        // }
        return true;
    }

    public async Task<object> Login(LoginRequestDto body)
    {
        ApplicationUser? user;

        user = await _userManager.FindByEmailAsync(body.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, body.Password))
        {
            throw new AppException("Invalid credentials", 401);
        }

        var tokenResponse = await _tokenService.GenerateTokenWithRefreshAsync(user);

        var data = new
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.AccessTokenExpiry,
            RefreshTokenExpiresIn = tokenResponse.RefreshTokenExpiry,
            User = new
            {
                Id = user?.Id,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            }
        };

        return data;
    }

    public async Task<object> AdminLogin(LoginRequestDto body)
    {
        var user = await _userManager.FindByEmailAsync(body.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, body.Password))
        {
            throw new AppException("Invalid credentials", 401);
        }

        var tokenResponse = await _tokenService.GenerateTokenWithRefreshAsync(user);

        var data = new
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = tokenResponse.TokenType,
            ExpiresIn = tokenResponse.AccessTokenExpiry,
            RefreshTokenExpiresIn = tokenResponse.RefreshTokenExpiry,
            User = new
            {
                Id = user?.Id,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            }
        };

        return data;
    }

    public bool VerifyOtp(VerifyOtpRequestDto body)
    {
        if (body.Otp != "1234")
        {
            throw new AppException("Invalid OTP", 400);
        }
        return true;
    }

    public async Task<bool> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user)
    {
        if (body is null)
        {
            throw new AppException("Request body cannot be null", 400);
        }

        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new AppException("User not found", 404);
        }

        var loan = await _loanRepository.GetLoanById(body.LoanId);

        if (loan == null || loan.UserId != userId)
        {
            throw new AppException("Loan not found for user", 404);
        }

        var existingEmployee = await _employeeRepository.ExistsAsync(userId, body.LoanId);

        if (existingEmployee)
        {
            throw new AppException("Personal details already submitted for this loan", 409);
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Employer = body.Employer,
            Industry = body.Industry,
            Role = body.Role,
            ResidentialAddress = body.ResidentialAddress,
            LoanId = body.LoanId
        };

        var result = await _employeeRepository.CreateEmployeeAsync(employee);
        
        if (!result)
        {
            throw new AppException("Failed to save personal details", 500);
        }

        return true;
    }

    public async Task<object> CreateSuperAdmin(CreateSuperAdminRequestDto body)
    {
        var user = new ApplicationUser
        {
            FirstName = body.FirstName,
            LastName = body.LastName,
            Email = body.Email,
            UserName = body.Email,
            LastLoginAt = DateTime.UtcNow,
        };

        var result = await _userManager.CreateAsync(user, body.Password);

        if (!result.Succeeded)
        {
            throw new AppException("Failed to create super admin: " + string.Join(", ", result.Errors.Select(e => e.Description)), 400);
        }

        // Assign SuperAdmin role
        await _userManager.AddToRoleAsync(user, "SuperAdmin");

        return new { userId = user.Id };
    }

    public async Task<object> CreateAdmin(CreateAdminRequestDto body)
    {
        var user = new ApplicationUser
        {
            FirstName = body.FirstName,
            LastName = body.LastName,
            Email = body.Email,
            UserName = body.Email,
            CompanyId = body.CompanyId.ToString()
        };

        var result = await _userManager.CreateAsync(user, body.Password);

        if (!result.Succeeded)
        {
            throw new AppException("Failed to create admin: " + string.Join(", ", result.Errors.Select(e => e.Description)), 400);
        }

        // Assign Admin role
        await _userManager.AddToRoleAsync(user, "Admin");

        return new { userId = user.Id };
    }

    public Task<object> GetRoles()
    {
        var roles = _roleManager.Roles.Select(r => new
        {
            r.Id,
            r.Name
        })
        .ToList();

        return Task.FromResult((object)roles);
    }

    public async Task<object> AssignRole(RoleAssignDto body)
    {
        var user = await _userManager.FindByIdAsync(body.UserId) ?? throw new AppException("User not found", 404);
        var role = await _roleManager.FindByIdAsync(body.RoleId);

        if (role is null)
        {
            throw new AppException("Role not found", 404);
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name!);

        return result;
    }

    public async Task<object> RefreshToken(RefreshTokenRequestDto request)
    {
        var tokenResponse = await _tokenService.RefreshTokenAsync(request.RefreshToken);

        return tokenResponse;

    }

    public async Task<bool> RevokeToken(RevokeTokenRequestDto request, string? userId = null)
    {
        var result = await _tokenService.RevokeTokenAsync(request.RefreshToken, userId, request.Reason);
        return result;
    }

    public async Task<bool> Logout(string? userId = null)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new AppException("User ID is required for logout", 400);
        }

        await _tokenService.RevokeAllUserTokensAsync(userId, userId, "User logged out");
        return true;
    }

    public async Task<object> GetSuperAdminDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);

        // Get all companies
        var allCompanies = await _companyRepository.GetAllCompanies();
        var companiesList = allCompanies.ToList();

        // Get all users
        var allUsers = await _userRepository.GetAllUsersAsync();

        // Get all loans
        var allLoans = new List<Loan>();
        foreach (var company in companiesList)
        {
            var companyLoans = await _loanRepository.GetAllLoansByCompanyId(company.Id);
            allLoans.AddRange(companyLoans);
        }

        // Get financial data
        var allDisbursements = await _disbursementRepository.GetAllDisbursements();
        var allRepayments = await _repaymentRepository.GetAllRepayments();
        var allSupportTickets = await _supportTicketRepository.GetAllAsync();

        // Calculate system-wide statistics
        var systemStats = CalculateSystemWideStatistics(companiesList, allUsers, allLoans, now, todayStart, weekStart, monthStart, yearStart);

        // Calculate company overviews
        var companyOverviews = await CalculateCompanyOverviews(companiesList, allLoans, allDisbursements);

        // Calculate system-wide analytics
        var analytics = CalculateSystemWideAnalytics(allUsers, allLoans, companiesList);

        // Calculate financial metrics
        var financialMetrics = CalculateSystemWideFinancialMetrics(allLoans, allDisbursements, allRepayments);

        // Calculate performance metrics
        var performanceMetrics = CalculatePlatformPerformanceMetrics(allSupportTickets.ToList());

        // Calculate growth trends
        var companyGrowthTrend = CalculateCompanyGrowthTrend(companiesList, now);
        var userGrowthTrend = CalculateUserGrowthTrend(allUsers, now);
        var loanVolumeTrend = CalculateLoanVolumeTrend(allLoans, now);
        var revenueGrowthTrend = CalculateRevenueGrowthTrend(allDisbursements, allRepayments, now);

        var dashboard = new SuperAdminDashboardDto
        {
            SystemStats = systemStats,
            Companies = companyOverviews,
            Analytics = analytics,
            FinancialMetrics = financialMetrics,
            Performance = performanceMetrics,
            CompanyGrowthTrend = companyGrowthTrend,
            UserGrowthTrend = userGrowthTrend,
            LoanVolumeTrend = loanVolumeTrend,
            RevenueGrowthTrend = revenueGrowthTrend
        };

        return dashboard;

    }

    private SystemWideStatistics CalculateSystemWideStatistics(
        List<Company> companies,
        List<ApplicationUser> users,
        List<Loan> loans,
        DateTime now,
        DateTime todayStart,
        DateTime weekStart,
        DateTime monthStart,
        DateTime yearStart)
    {
        return new SystemWideStatistics
        {
            TotalCompanies = companies.Count,
            ActiveCompanies = companies.Count(c => c.IsActive),
            TotalUsers = users.Count,
            TotalLoans = loans.Count,
            TodayUsers = users.Count(u => u.CreatedAt.Date >= todayStart),
            TodayLoans = loans.Count(l => l.CreatedAt.Date >= todayStart),
            ThisWeekUsers = users.Count(u => u.CreatedAt >= weekStart),
            ThisWeekLoans = loans.Count(l => l.CreatedAt >= weekStart),
            ThisMonthUsers = users.Count(u => u.CreatedAt >= monthStart),
            ThisMonthLoans = loans.Count(l => l.CreatedAt >= monthStart),
            ThisYearUsers = users.Count(u => u.CreatedAt >= yearStart),
            ThisYearLoans = loans.Count(l => l.CreatedAt >= yearStart)
        };
    }

    private async Task<List<CompanyOverviewDto>> CalculateCompanyOverviews(
        List<Company> companies,
        List<Loan> allLoans,
        List<Disbursement> allDisbursements)
    {
        var overviews = new List<CompanyOverviewDto>();

        foreach (var company in companies)
        {
            var companyUsers = await _userRepository.GetUsersByCompanyIdAsync(company.Id.ToString());

            var companyLoans = allLoans.Where(l => l.CompanyId == company.Id).ToList();
            var companyDisbursements = allDisbursements.Where(d =>
                companyLoans.Any(l => l.Id.ToString() == d.LoanId)).ToList();

            var defaultedLoans = companyLoans.Count(l => l.Status == LoanStatus.Overdue);
            var totalLoans = companyLoans.Count;

            overviews.Add(new CompanyOverviewDto
            {
                Id = company.Id,
                Name = company.Name,
                ShortName = company.ShortName,
                IsActive = company.IsActive,
                TotalUsers = companyUsers.Count,
                TotalLoans = totalLoans,
                TotalLoanAmount = companyLoans.Sum(l => l.Amount),
                TotalDisbursed = companyDisbursements.Sum(d => d.Amount),
                DefaultRate = totalLoans > 0 ? Math.Round((double)defaultedLoans / totalLoans * 100, 2) : 0,
                CreatedAt = company.CreatedAt,
                LastActivity = companyLoans.Any() ? companyLoans.Max(l => l.CreatedAt) : company.CreatedAt
            });
        }

        return overviews.OrderByDescending(c => c.TotalLoanAmount).ToList();
    }

    private SystemWideAnalytics CalculateSystemWideAnalytics(
        List<ApplicationUser> users,
        List<Loan> loans,
        List<Company> companies)
    {
        // Gender distribution
        var totalUsers = users.Count;
        var maleCount = users.Count(u => u.Gender?.ToLower() == "male");
        var femaleCount = users.Count(u => u.Gender?.ToLower() == "female");

        var genderDistribution = new GenderDistribution
        {
            MaleCount = maleCount,
            FemaleCount = femaleCount,
            MalePercentage = totalUsers > 0 ? Math.Round((double)maleCount / totalUsers * 100, 2) : 0,
            FemalePercentage = totalUsers > 0 ? Math.Round((double)femaleCount / totalUsers * 100, 2) : 0,
            TotalUsers = totalUsers
        };

        // Loan status distribution
        var totalLoans = loans.Count;
        var pendingLoans = loans.Count(l => l.Status == LoanStatus.Pending);
        var approvedLoans = loans.Count(l => l.Status == LoanStatus.Approved);
        var disbursedLoans = loans.Count(l => l.Status == LoanStatus.Disbursed);
        var repaidLoans = loans.Count(l => l.Status == LoanStatus.Repaid);
        var overdueLoans = loans.Count(l => l.Status == LoanStatus.Overdue);
        var rejectedLoans = loans.Count(l => l.Status == LoanStatus.Rejected);

        var loanStatusDistribution = new LoanStatusDistribution
        {
            PendingLoans = pendingLoans,
            ApprovedLoans = approvedLoans,
            DisbursedLoans = disbursedLoans,
            RepaidLoans = repaidLoans,
            OverdueLoans = overdueLoans,
            RejectedLoans = rejectedLoans,
            PendingPercentage = totalLoans > 0 ? Math.Round((double)pendingLoans / totalLoans * 100, 2) : 0,
            ApprovedPercentage = totalLoans > 0 ? Math.Round((double)approvedLoans / totalLoans * 100, 2) : 0,
            DisbursedPercentage = totalLoans > 0 ? Math.Round((double)disbursedLoans / totalLoans * 100, 2) : 0,
            RepaidPercentage = totalLoans > 0 ? Math.Round((double)repaidLoans / totalLoans * 100, 2) : 0,
            OverduePercentage = totalLoans > 0 ? Math.Round((double)overdueLoans / totalLoans * 100, 2) : 0,
            RejectedPercentage = totalLoans > 0 ? Math.Round((double)rejectedLoans / totalLoans * 100, 2) : 0
        };

        // Company performance metrics
        var companyPerformanceMetrics = CalculateCompanyPerformanceMetrics(companies, loans);

        // Company risk analysis
        var companyRiskAnalysis = CalculateCompanyRiskAnalysis(companies, loans);

        return new SystemWideAnalytics
        {
            UserGenderBreakdown = genderDistribution,
            LoanStatusBreakdown = loanStatusDistribution,
            TopPerformingCompanies = companyPerformanceMetrics,
            CompanyRiskAnalysis = companyRiskAnalysis
        };
    }

    private CompanyPerformanceMetrics CalculateCompanyPerformanceMetrics(List<Company> companies, List<Loan> loans)
    {
        var companyLoanVolumes = companies.Select(c => new CompanyPerformanceDto
        {
            CompanyId = c.Id,
            CompanyName = c.Name,
            Value = loans.Where(l => l.CompanyId == c.Id).Sum(l => l.Amount),
            Metric = "Loan Volume"
        }).OrderByDescending(c => c.Value).Take(10).ToList();

        // Calculate percentages
        var totalVolume = companyLoanVolumes.Sum(c => c.Value);
        companyLoanVolumes.ForEach(c => c.Percentage = totalVolume > 0 ? Math.Round((double)(c.Value / totalVolume) * 100, 2) : 0);

        return new CompanyPerformanceMetrics
        {
            ByLoanVolume = companyLoanVolumes,
            ByUserCount = new List<CompanyPerformanceDto>(), // Will be calculated with user data
            ByRevenue = new List<CompanyPerformanceDto>(), // Will be calculated with disbursement data
            ByGrowthRate = new List<CompanyPerformanceDto>() // Will be calculated with time-based data
        };
    }

    private List<CompanyRiskMetrics> CalculateCompanyRiskAnalysis(List<Company> companies, List<Loan> loans)
    {
        return companies.Select(c =>
        {
            var companyLoans = loans.Where(l => l.CompanyId == c.Id).ToList();
            var totalLoans = companyLoans.Count;
            var defaultedLoans = companyLoans.Count(l => l.Status == LoanStatus.Overdue);
            var overdueLoans = companyLoans.Count(l => l.Status == LoanStatus.Overdue);

            var defaultRate = totalLoans > 0 ? (double)defaultedLoans / totalLoans * 100 : 0;
            var overdueRate = totalLoans > 0 ? (double)overdueLoans / totalLoans * 100 : 0;

            var riskLevel = defaultRate >= 15 ? "Critical" :
                           defaultRate >= 10 ? "High" :
                           defaultRate >= 5 ? "Medium" : "Low";

            return new CompanyRiskMetrics
            {
                CompanyId = c.Id,
                CompanyName = c.Name,
                DefaultRate = Math.Round(defaultRate, 2),
                OverdueRate = Math.Round(overdueRate, 2),
                TotalLoans = totalLoans,
                ExposureAmount = companyLoans.Sum(l => l.Amount),
                RiskLevel = riskLevel
            };
        }).OrderByDescending(c => c.DefaultRate).ToList();
    }

    private SystemWideFinancialMetrics CalculateSystemWideFinancialMetrics(
        List<Loan> loans,
        List<Disbursement> disbursements,
        List<Repayment> repayments)
    {
        var totalRequested = loans.Sum(l => l.Amount);
        var totalDisbursed = disbursements.Sum(d => d.Amount);
        var totalRepayments = repayments.Sum(r => r.Amount);
        var outstandingAmount = totalDisbursed - totalRepayments;

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var yearStart = new DateTime(DateTime.UtcNow.Year, 1, 1);

        var monthlyDisbursements = disbursements.Where(d => d.CreatedAt >= monthStart).Sum(d => d.Amount);
        var monthlyRepayments = repayments.Where(r => r.CreatedAt >= monthStart).Sum(r => r.Amount);

        var totalProcessedLoans = loans.Count(l => l.Status == LoanStatus.Approved || l.Status == LoanStatus.Rejected);
        var defaultedLoans = loans.Count(l => l.Status == LoanStatus.Overdue);

        return new SystemWideFinancialMetrics
        {
            TotalLoanRequests = totalRequested,
            TotalDisbursed = totalDisbursed,
            TotalRepayments = totalRepayments,
            OutstandingAmount = outstandingAmount,
            AverageLoanSize = loans.Count > 0 ? totalRequested / loans.Count : 0,
            SystemRepaymentRate = totalDisbursed > 0 ? Math.Round((double)(totalRepayments / totalDisbursed) * 100, 2) : 0,
            SystemDefaultRate = totalProcessedLoans > 0 ? Math.Round((double)defaultedLoans / totalProcessedLoans * 100, 2) : 0,
            MonthlyDisbursementVolume = monthlyDisbursements,
            MonthlyRepaymentVolume = monthlyRepayments,
            RevenueThisMonth = monthlyDisbursements * 0.1m, // Assuming 10% revenue rate
            RevenueThisYear = disbursements.Where(d => d.CreatedAt >= yearStart).Sum(d => d.Amount) * 0.1m
        };
    }

    private PlatformPerformanceMetrics CalculatePlatformPerformanceMetrics(List<SupportTicket> supportTickets)
    {
        var totalTickets = supportTickets.Count;
        var openTickets = supportTickets.Count(t => t.Status?.ToLower() == "open" || t.Status?.ToLower() == "inprogress");
        var resolvedTickets = supportTickets.Count(t => t.Status?.ToLower() == "resolved" || t.Status?.ToLower() == "closed");

        return new PlatformPerformanceMetrics
        {
            TotalSupportTickets = totalTickets,
            OpenTickets = openTickets,
            ResolvedTickets = resolvedTickets,
            TicketResolutionRate = totalTickets > 0 ? Math.Round((double)resolvedTickets / totalTickets * 100, 2) : 0,
            AverageResolutionTimeHours = 24.5, // This would be calculated from actual resolution times
            SystemUptime = 99, // This would come from monitoring systems
            UserSatisfactionScore = 4.2, // This would come from user feedback
            TotalTransactions = supportTickets.Count * 10, // Placeholder calculation
            TransactionSuccessRate = 98.5 // This would come from transaction logs
        };
    }

    private List<GraphDataPoint> CalculateCompanyGrowthTrend(List<Company> companies, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date.AddDays(1 - now.AddMonths(-i).Day);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var companiesThisMonth = companies.Count(c => c.CreatedAt >= monthStart && c.CreatedAt <= monthEnd);

            result.Add(new GraphDataPoint
            {
                Label = monthStart.ToString("MMM yyyy"),
                Value = companiesThisMonth,
                Date = monthStart
            });
        }
        return result;
    }

    private static List<GraphDataPoint> CalculateUserGrowthTrend(List<ApplicationUser> users, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date.AddDays(1 - now.AddMonths(-i).Day);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var usersThisMonth = users.Count(u => u.CreatedAt >= monthStart && u.CreatedAt <= monthEnd);

            result.Add(new GraphDataPoint
            {
                Label = monthStart.ToString("MMM yyyy"),
                Value = usersThisMonth,
                Date = monthStart
            });
        }
        return result;
    }

    private List<GraphDataPoint> CalculateLoanVolumeTrend(List<Loan> loans, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date.AddDays(1 - now.AddMonths(-i).Day);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var volumeThisMonth = loans
                .Where(l => l.CreatedAt >= monthStart && l.CreatedAt <= monthEnd)
                .Sum(l => l.Amount);

            result.Add(new GraphDataPoint
            {
                Label = monthStart.ToString("MMM yyyy"),
                Value = volumeThisMonth,
                Date = monthStart
            });
        }
        return result;
    }

    private List<GraphDataPoint> CalculateRevenueGrowthTrend(List<Disbursement> disbursements, List<Repayment> repayments, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date.AddDays(1 - now.AddMonths(-i).Day);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var disbursedThisMonth = disbursements
                .Where(d => d.CreatedAt >= monthStart && d.CreatedAt <= monthEnd)
                .Sum(d => d.Amount);

            // Assuming revenue is 10% of disbursed amount
            var revenueThisMonth = disbursedThisMonth * 0.1m;

            result.Add(new GraphDataPoint
            {
                Label = monthStart.ToString("MMM yyyy"),
                Value = revenueThisMonth,
                Date = monthStart
            });
        }
        return result;
    }

    public async Task<object> GetAdminListAsync(AdminFilterDto filter)
    {
        var adminRoles = new[] { "Admin", "SuperAdmin" };
        var adminUsers = new List<ApplicationUser>();

        foreach (var role in adminRoles)
        {
            var usersInRole = await _userRepository.GetUsersInRoleAsync(role);
            adminUsers.AddRange(usersInRole);
        }

        // Remove duplicates (users with multiple admin roles)
        adminUsers = adminUsers.DistinctBy(u => u.Id).ToList();

        // Apply filters
        var query = adminUsers.AsQueryable();

        // Search filter (name or email)
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)));
        }

        // Role filter
        if (!string.IsNullOrEmpty(filter.Role))
        {
            var usersInFilterRole = await _userRepository.GetUsersInRoleAsync(filter.Role);
            var userIdsInRole = usersInFilterRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => userIdsInRole.Contains(u.Id));
        }

        // Company filter
        if (!string.IsNullOrEmpty(filter.CompanyId))
        {
            query = query.Where(u => u.CompanyId == filter.CompanyId);
        }

        // Gender filter
        if (!string.IsNullOrEmpty(filter.Gender))
        {
            query = query.Where(u => u.Gender == filter.Gender);
        }

        // Active status filter
        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        // Get total count before pagination
        var totalCount = query.Count();

        // Apply sorting
        query = filter.SortBy?.ToLower() switch
        {
            "firstname" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(u => u.FirstName)
                : query.OrderBy(u => u.FirstName),
            "lastname" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(u => u.LastName)
                : query.OrderBy(u => u.LastName),
            "email" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            "createdat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(u => u.CreatedAt)
                : query.OrderBy(u => u.CreatedAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };

        // Apply pagination
        var pagedUsers = query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        // Map to DTOs with additional data
        var adminDtos = new List<AdminListDto>();

        foreach (var user in pagedUsers)
        {
            var userRoles = await _userManager.GetRolesAsync(user);
            var company = user.CompanyId != null
                ? await _companyRepository.GetCompanyById(Guid.Parse(user.CompanyId))
                : null;

            adminDtos.Add(new AdminListDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = string.Join(", ", userRoles),
                CompanyName = company?.Name,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            });
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        var result = new PagedAdminListDto
        {
            Admins = adminDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1
        };

        return result;

    }
}

