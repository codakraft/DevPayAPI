using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class SupportService : ISupportService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILoanRepository _loanRepository;
    private readonly IDisbursementRepository _disbursementRepository;
    private readonly IRepaymentRepository _repaymentRepository;
    private readonly ISupportTicketRepository _supportTicketRepository;
    private readonly ISupportCommentRepository _supportCommentRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<SupportService> _logger;

    public SupportService(
        UserManager<ApplicationUser> userManager,
        ILoanRepository loanRepository,
        IDisbursementRepository disbursementRepository,
        IRepaymentRepository repaymentRepository,
        ISupportTicketRepository supportTicketRepository,
        ISupportCommentRepository supportCommentRepository,
        ICompanyRepository companyRepository,
        ILogger<SupportService> logger)
    {
        _userManager = userManager;
        _loanRepository = loanRepository;
        _disbursementRepository = disbursementRepository;
        _repaymentRepository = repaymentRepository;
        _supportTicketRepository = supportTicketRepository;
        _supportCommentRepository = supportCommentRepository;
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketDto dto, string userId)
    {
        // Get user to retrieve company information
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new AppException("User not found", 404);
        }

        if (string.IsNullOrEmpty(user.CompanyId) || !Guid.TryParse(user.CompanyId, out var companyGuid))
        {
            throw new AppException("User is not associated with a valid company", 400);
        }

        // Create new support ticket entity
        var ticket = new SupportTicket
        {
            UserId = userId,
            CompanyId = companyGuid,
            Subject = dto.Subject,
            Description = dto.Description,
            Category = dto.Category,
            Priority = dto.Priority,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        var createdTicket = await _supportTicketRepository.CreateAsync(ticket);

        // Convert to DTO for response
        return new SupportTicketDto
        {
            Id = createdTicket.Id,
            UserId = createdTicket.UserId,
            UserName = $"{user.FirstName} {user.LastName}",
            UserEmail = user.Email ?? "",
            Subject = createdTicket.Subject,
            Description = createdTicket.Description,
            Category = createdTicket.Category,
            Priority = createdTicket.Priority,
            Status = createdTicket.Status,
            AssignedTo = createdTicket.AssignedTo,
            CreatedAt = createdTicket.CreatedAt,
            ResolvedAt = createdTicket.ResolvedAt,
            Comments = new List<SupportCommentDto>()
        };
    }

    public async Task<List<SupportTicketDto>> GetAllTicketsAsync(Guid? companyScope = null)
    {
        var tickets = (await _supportTicketRepository.GetAllAsync()).Where(t => InScope(companyScope, t.CompanyId));
        var ticketDtos = tickets.Select(ticket => new SupportTicketDto
        {
            Id = ticket.Id,
            UserId = ticket.UserId,
            UserName = $"{ticket.User.FirstName} {ticket.User.LastName}",
            UserEmail = ticket.User.Email ?? "",
            Subject = ticket.Subject,
            Description = ticket.Description,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedTo = ticket.AssignedTo,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Comments = ticket.Comments.Select(c => new SupportCommentDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UserId = c.UserId,
                UserName = $"{c.User.FirstName} {c.User.LastName}",
                Comment = c.Comment,
                IsInternal = c.IsInternal,
                CreatedAt = c.CreatedAt
            }).ToList()
        }).ToList();

        return ticketDtos;
    }

    public async Task<SupportTicketDto> GetTicketByIdAsync(string ticketId, Guid? companyScope = null)
    {
        var ticket = await _supportTicketRepository.GetByIdWithCommentsAsync(ticketId);
        if (ticket == null || !InScope(companyScope, ticket.CompanyId))
        {
            throw new AppException("Support ticket not found", 404);
        }

        return new SupportTicketDto
        {
            Id = ticket.Id,
            UserId = ticket.UserId,
            UserName = $"{ticket.User.FirstName} {ticket.User.LastName}",
            UserEmail = ticket.User.Email ?? "",
            Subject = ticket.Subject,
            Description = ticket.Description,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedTo = ticket.AssignedTo,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Comments = ticket.Comments.Select(c => new SupportCommentDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UserId = c.UserId,
                UserName = $"{c.User.FirstName} {c.User.LastName}",
                Comment = c.Comment,
                IsInternal = c.IsInternal,
                CreatedAt = c.CreatedAt
            }).ToList()
        };
    }

    public async Task UpdateTicketAsync(string ticketId, UpdateSupportTicketDto dto, string userId)
    {
        // Mock implementation
        await Task.CompletedTask; // Add await to resolve warning
    }

    public async Task<SupportCommentDto> AddCommentToTicketAsync(string ticketId, AddSupportCommentDto dto, string userId, Guid? companyScope = null)
    {
        // Verify ticket exists
        var ticket = await _supportTicketRepository.GetByIdAsync(ticketId);
        if (ticket == null || !InScope(companyScope, ticket.CompanyId))
        {
            throw new AppException("Support ticket not found", 404);
        }

        // Get user for comment
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new AppException("User not found", 404);
        }

        // Create comment entity
        var comment = new SupportComment
        {
            TicketId = ticketId,
            UserId = userId,
            Comment = dto.Comment,
            IsInternal = dto.IsInternal,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        var createdComment = await _supportCommentRepository.CreateAsync(comment);

        // Convert to DTO for response
        return new SupportCommentDto
        {
            Id = createdComment.Id,
            TicketId = createdComment.TicketId,
            UserId = createdComment.UserId,
            UserName = $"{user.FirstName} {user.LastName}",
            Comment = createdComment.Comment,
            IsInternal = createdComment.IsInternal,
            CreatedAt = createdComment.CreatedAt
        };
    }

    public async Task<List<SupportTicketDto>> GetTicketsByUserIdAsync(string userId)
    {
        // Mock implementation
        await Task.CompletedTask; // Add await to resolve warning
        return new List<SupportTicketDto>();
    }

    public async Task<UserAccountSupportDto> GetUserAccountDetailsAsync(string userId, Guid? companyScope = null)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new AppException("User not found", 404);
        }

        var userLoans = (await _loanRepository.GetLoansByUserIdAsync(userId))
            .Where(l => InScope(companyScope, l.CompanyId))
            .ToList();
        if (!UserInScope(companyScope, user, userLoans))
        {
            throw new AppException("User not found", 404);
        }
        var totalLoanAmount = userLoans.Sum(l => l.Amount);
        var activeLoans = userLoans.Where(l => l.Status == Core.Enum.LoanStatus.Approved || 
                                               l.Status == Core.Enum.LoanStatus.Disbursed);

        return new UserAccountSupportDto
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber ?? "",
            IsActive = user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow, // Would need to track this in user entity
            TotalLoans = userLoans.Count(),
            ActiveLoans = activeLoans.Count(),
            TotalLoanAmount = totalLoanAmount,
            OutstandingAmount = 0 // Would need to calculate based on repayments
        };
    }

    public async Task<object> SearchUsersAsync(string searchTerm, Guid? companyScope = null)
    {
        var query = _userManager.Users.AsQueryable();
        if (companyScope.HasValue)
        {
            // Staff of the company, plus borrowers who have a loan with it
            var scope = companyScope.Value.ToString();
            var borrowerIds = (await _loanRepository.GetAllLoansByCompanyId(companyScope.Value))
                .Where(l => l.UserId != null)
                .Select(l => l.UserId!)
                .Distinct()
                .ToList();
            query = query.Where(u => u.CompanyId == scope || borrowerIds.Contains(u.Id));
        }

        var users = await query
            .Where(u => u.Email!.Contains(searchTerm) || 
                       u.FirstName.Contains(searchTerm) || 
                       u.LastName.Contains(searchTerm))
            .Take(20)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.PhoneNumber
            })
            .ToListAsync(); // Use async version

        return users;
    }

    public async Task PerformUserActionAsync(string userId, UserActionDto dto, string performedBy)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new AppException("User not found", 404);
        }

        IdentityResult result;
        switch (dto.Action.ToLower())
        {
            case "suspend":
                result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(10));
                break;
            case "activate":
                result = await _userManager.SetLockoutEndDateAsync(user, null);
                break;
            case "resetpassword":
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // In real implementation, would send reset email
                result = IdentityResult.Success;
                break;
            default:
                throw new AppException("Invalid action specified", 400);
        }

        if (!result.Succeeded)
        {
            throw new AppException($"Failed to perform action: {string.Join(", ", result.Errors.Select(e => e.Description))}", 500);
        }
    }

    public async Task<List<LoanSupportDto>> GetUserLoansAsync(string userId, Guid? companyScope = null)
    {
        var loans = (await _loanRepository.GetLoansByUserIdAsync(userId))
            .Where(l => InScope(companyScope, l.CompanyId))
            .ToList();
        var loanSupportDtos = new List<LoanSupportDto>();

        foreach (var loan in loans)
        {
            var disbursements = await _disbursementRepository.GetDisbursementsByLoanId(loan.Id.ToString());
            var repayments = await _repaymentRepository.GetRepaymentsByLoanId(loan.Id.ToString());

            var totalDisbursed = disbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
            var totalRepaid = repayments.Sum(r => r.TotalRepaid);

            loanSupportDtos.Add(new LoanSupportDto
            {
                LoanId = loan.Id.ToString(),
                UserId = loan.UserId ?? string.Empty,
                Amount = loan.Amount,
                DurationInMonths = loan.DurationInMonths,
                Purpose = loan.Purpose,
                Status = loan.Status.ToString(),
                CreatedAt = loan.CreatedAt,
                ApprovedAt = loan.ApprovedAt,
                DueDate = loan.DueDate,
                TotalDisbursed = totalDisbursed,
                TotalRepaid = totalRepaid,
                OutstandingAmount = totalDisbursed - totalRepaid
            });
        }

        return loanSupportDtos;
    }

    public async Task<LoanSupportDto> GetLoanDetailsAsync(string loanId)
    {
        var loan = await _loanRepository.GetLoanById(Guid.Parse(loanId));
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        var disbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
        var repayments = await _repaymentRepository.GetRepaymentsByLoanId(loanId);

        var totalDisbursed = disbursements.Where(d => d.Status == "Disbursed").Sum(d => d.Amount);
        var totalRepaid = repayments.Sum(r => r.TotalRepaid);

        return new LoanSupportDto
        {
            LoanId = loan.Id.ToString(),
            UserId = loan.UserId ?? string.Empty,
            Amount = loan.Amount,
            DurationInMonths = loan.DurationInMonths,
            Purpose = loan.Purpose,
            Status = loan.Status.ToString(),
            CreatedAt = loan.CreatedAt,
            ApprovedAt = loan.ApprovedAt,
            DueDate = loan.DueDate,
            TotalDisbursed = totalDisbursed,
            TotalRepaid = totalRepaid,
            OutstandingAmount = totalDisbursed - totalRepaid
        };
    }

    public async Task<object> SearchLoansAsync(string searchTerm, Guid? companyScope = null)
    {
        var allLoans = (await _loanRepository.GetAllLoans()).Where(l => InScope(companyScope, l.CompanyId));
        var filteredLoans = allLoans
            .Where(l => l.Id.ToString().Contains(searchTerm) || 
                       (l.UserId != null && l.UserId.Contains(searchTerm)) ||
                       l.Purpose.Contains(searchTerm))
            .Take(20)
            .Select(l => new
            {
                l.Id,
                l.UserId,
                l.Amount,
                l.Purpose,
                Status = l.Status.ToString(),
                l.CreatedAt
            })
            .ToList();

        return filteredLoans;
    }

    public async Task<object> GetLoansByStatusAsync(string status, Guid? companyScope = null)
    {
        var allLoans = (await _loanRepository.GetAllLoans()).Where(l => InScope(companyScope, l.CompanyId));
        var filteredLoans = allLoans
            .Where(l => l.Status.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return filteredLoans;
    }

    public async Task<object> GetOverdueLoansAsync(Guid? companyScope = null)
    {
        var allLoans = (await _loanRepository.GetAllLoans()).Where(l => InScope(companyScope, l.CompanyId));
        var overdueLoans = allLoans
            .Where(l => l.DueDate.HasValue && l.DueDate < DateTime.UtcNow && 
                       l.Status == Core.Enum.LoanStatus.Approved)
            .ToList();

        return overdueLoans;
    }

    public async Task<SupportDashboardDto> GetSupportDashboardAsync(Guid? companyScope = null)
    {
        if (companyScope.HasValue)
        {
            return await GetCompanySupportDashboardAsync(companyScope.Value);
        }

        // Get dashboard statistics from repository
        var allTickets = await _supportTicketRepository.GetAllAsync();
        var totalTickets = allTickets.Count();
        var openTickets = await _supportTicketRepository.GetTicketCountByStatusAsync("Open");
        var inProgressTickets = await _supportTicketRepository.GetTicketCountByStatusAsync("InProgress");
        var resolvedTickets = await _supportTicketRepository.GetTicketCountByStatusAsync("Resolved");
        var highPriorityTickets = await _supportTicketRepository.GetTicketCountByPriorityAsync("High");
        var criticalPriorityTickets = await _supportTicketRepository.GetTicketCountByPriorityAsync("Critical");
        var averageResolutionTime = await _supportTicketRepository.GetAverageResolutionTimeHoursAsync();
        var recentTickets = await _supportTicketRepository.GetRecentTicketsAsync(5);

        return new SupportDashboardDto
        {
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            InProgressTickets = inProgressTickets,
            ResolvedTickets = resolvedTickets,
            HighPriorityTickets = highPriorityTickets,
            CriticalPriorityTickets = criticalPriorityTickets,
            AverageResolutionTimeHours = averageResolutionTime,
            RecentTickets = ConvertTicketsToDto(recentTickets)
        };
    }

    public async Task<List<SupportTicketDto>> GetTicketsByStatusAsync(string status, Guid? companyScope = null)
    {
        var tickets = (await _supportTicketRepository.GetByStatusAsync(status)).Where(t => InScope(companyScope, t.CompanyId));
        var ticketDtos = tickets.Select(ticket => new SupportTicketDto
        {
            Id = ticket.Id,
            UserId = ticket.UserId,
            UserName = $"{ticket.User.FirstName} {ticket.User.LastName}",
            UserEmail = ticket.User.Email ?? "",
            Subject = ticket.Subject,
            Description = ticket.Description,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedTo = ticket.AssignedTo,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Comments = new List<SupportCommentDto>() // Comments not loaded for list views
        }).ToList();

        return ticketDtos;
    }

    public async Task<List<SupportTicketDto>> GetTicketsByCategoryAsync(string category, Guid? companyScope = null)
    {
        var tickets = (await _supportTicketRepository.GetByCategoryAsync(category)).Where(t => InScope(companyScope, t.CompanyId));
        var ticketDtos = ConvertTicketsToDto(tickets);
        return ticketDtos;
    }

    public async Task<List<SupportTicketDto>> GetTicketsByPriorityAsync(string priority, Guid? companyScope = null)
    {
        var tickets = (await _supportTicketRepository.GetByPriorityAsync(priority)).Where(t => InScope(companyScope, t.CompanyId));
        var ticketDtos = ConvertTicketsToDto(tickets);
        return ticketDtos;
    }

    private static bool InScope(Guid? companyScope, Guid companyId) =>
        companyScope is null || companyScope == companyId;

    /// <summary>
    /// A user is visible to a company if they are its staff or have a loan with it.
    /// </summary>
    private static bool UserInScope(Guid? companyScope, ApplicationUser user, IEnumerable<Loan> loansInScope) =>
        companyScope is null
        || user.CompanyId == companyScope.Value.ToString()
        || loansInScope.Any();

    /// <summary>
    /// Same figures as the platform dashboard, computed over one company's tickets.
    /// </summary>
    private async Task<SupportDashboardDto> GetCompanySupportDashboardAsync(Guid companyId)
    {
        var tickets = (await _supportTicketRepository.GetByCompanyIdAsync(companyId)).ToList();
        var resolved = tickets.Where(t => t.ResolvedAt.HasValue).ToList();

        return new SupportDashboardDto
        {
            TotalTickets = tickets.Count,
            OpenTickets = tickets.Count(t => t.Status == "Open"),
            InProgressTickets = tickets.Count(t => t.Status == "InProgress"),
            ResolvedTickets = tickets.Count(t => t.Status == "Resolved"),
            HighPriorityTickets = tickets.Count(t => t.Priority == "High"),
            CriticalPriorityTickets = tickets.Count(t => t.Priority == "Critical"),
            AverageResolutionTimeHours = resolved.Count == 0
                ? 0
                : resolved.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours),
            RecentTickets = ConvertTicketsToDto(tickets.OrderByDescending(t => t.CreatedAt).Take(5))
        };
    }

    // Helper method to convert tickets to DTOs
    private List<SupportTicketDto> ConvertTicketsToDto(IEnumerable<SupportTicket> tickets, bool includeComments = false)
    {
        return tickets.Select(ticket => new SupportTicketDto
        {
            Id = ticket.Id,
            UserId = ticket.UserId,
            UserName = $"{ticket.User.FirstName} {ticket.User.LastName}",
            UserEmail = ticket.User.Email ?? "",
            Subject = ticket.Subject,
            Description = ticket.Description,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedTo = ticket.AssignedTo,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            Comments = includeComments ? ticket.Comments.Select(c => new SupportCommentDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                UserId = c.UserId,
                UserName = $"{c.User.FirstName} {c.User.LastName}",
                Comment = c.Comment,
                IsInternal = c.IsInternal,
                CreatedAt = c.CreatedAt
            }).ToList() : new List<SupportCommentDto>()
        }).ToList();
    }

    public async Task<CompanyDashboardDto> GetCompanyDashboardAsync(string companyId)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
        {
            throw new AppException("Invalid company ID format", 400);
        }

            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            // Get all users for the company
            var allUsers = await _userManager.Users
                .Where(u => u.CompanyId == companyId)
                .ToListAsync();

            // Calculate user statistics
            var userStats = new UserStatistics
            {
                AllTime = allUsers.Count,
                Today = allUsers.Count(u => u.CreatedAt.Date >= todayStart),
                ThisWeek = allUsers.Count(u => u.CreatedAt >= weekStart),
                ThisMonth = allUsers.Count(u => u.CreatedAt >= monthStart),
                ThisYear = allUsers.Count(u => u.CreatedAt >= yearStart)
            };

            // Calculate user analytics (gender distribution)
            var userAnalytics = CalculateUserAnalytics(allUsers);

            // Get all loans for the company
            var allLoans = await _loanRepository.GetAllLoansByCompanyId(companyGuid);
            var loansList = allLoans.ToList();

            // Calculate loan statistics
            var loanStats = new LoanStatistics
            {
                AllTime = loansList.Count,
                Today = loansList.Count(l => l.CreatedAt.Date >= todayStart),
                ThisWeek = loansList.Count(l => l.CreatedAt >= weekStart),
                ThisMonth = loansList.Count(l => l.CreatedAt >= monthStart),
                ThisYear = loansList.Count(l => l.CreatedAt >= yearStart)
            };

            // Get disbursement analytics
            var disbursementAnalytics = await CalculateDisbursementAnalytics(companyGuid, now);

            // Get loan request analytics
            var loanRequestAnalytics = CalculateLoanRequestAnalytics(loansList, now);

            // Calculate financial metrics
            var financialMetrics = await CalculateFinancialMetrics(companyGuid, loansList);

            // Calculate system metrics
            var systemMetrics = await CalculateSystemMetrics(companyGuid, allUsers, loansList);

            var dashboard = new CompanyDashboardDto
            {
                Users = userStats,
                Loans = loanStats,
                Analytics = userAnalytics,
                Disbursements = disbursementAnalytics,
                LoanRequests = loanRequestAnalytics,
                FinancialMetrics = financialMetrics,
                SystemMetrics = systemMetrics
            };

            return dashboard;
        }

    private UserAnalytics CalculateUserAnalytics(List<ApplicationUser> users)
    {
        var totalUsers = users.Count;
        if (totalUsers == 0)
        {
            return new UserAnalytics
            {
                MalePercentage = 0,
                FemalePercentage = 0,
                TotalUsers = 0
            };
        }

        var maleCount = users.Count(u => u.Gender?.ToLower() == "male");
        var femaleCount = users.Count(u => u.Gender?.ToLower() == "female");

        return new UserAnalytics
        {
            MalePercentage = Math.Round((double)maleCount / totalUsers * 100, 2),
            FemalePercentage = Math.Round((double)femaleCount / totalUsers * 100, 2),
            TotalUsers = totalUsers
        };
    }

    private async Task<DisbursementAnalytics> CalculateDisbursementAnalytics(Guid companyId, DateTime now)
    {
        // Get all disbursements for the company (through loan relationship)
        var allLoans = await _loanRepository.GetAllLoansByCompanyId(companyId);
        var loanIds = allLoans.Select(l => l.Id.ToString()).ToList();
        
        // Note: This would be more efficient with a direct query, but using available repository methods
        var disbursements = new List<Disbursement>();
        foreach (var loanId in loanIds)
        {
            var loanDisbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
            disbursements.AddRange(loanDisbursements);
        }

        return new DisbursementAnalytics
        {
            Last7Days = GenerateLast7DaysData(disbursements, now),
            CurrentMonthDaily = GenerateCurrentMonthDailyData(disbursements, now),
            CurrentMonthWeekly = GenerateCurrentMonthWeeklyData(disbursements, now),
            CurrentYearMonthly = GenerateCurrentYearMonthlyData(disbursements, now)
        };
    }

    private LoanRequestAnalytics CalculateLoanRequestAnalytics(List<Loan> loans, DateTime now)
    {
        return new LoanRequestAnalytics
        {
            Last7Days = GenerateLast7DaysLoanData(loans, now),
            CurrentMonthDaily = GenerateCurrentMonthDailyLoanData(loans, now),
            CurrentMonthWeekly = GenerateCurrentMonthWeeklyLoanData(loans, now),
            CurrentYearMonthly = GenerateCurrentYearMonthlyLoanData(loans, now)
        };
    }

    private List<GraphDataPoint> GenerateLast7DaysData(List<Disbursement> disbursements, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            var dayDisbursements = disbursements.Where(d => d.CreatedAt.Date == date);
            result.Add(new GraphDataPoint
            {
                Label = date.ToString("MMM dd"),
                Value = dayDisbursements.Sum(d => d.Amount),
                Date = date
            });
        }
        return result;
    }

    private List<GraphDataPoint> GenerateCurrentMonthDailyData(List<Disbursement> disbursements, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        for (int day = 1; day <= Math.Min(daysInMonth, now.Day); day++)
        {
            var date = new DateTime(now.Year, now.Month, day);
            var dayDisbursements = disbursements.Where(d => d.CreatedAt.Date == date);
            result.Add(new GraphDataPoint
            {
                Label = date.ToString("dd"),
                Value = dayDisbursements.Sum(d => d.Amount),
                Date = date
            });
        }
        return result;
    }

    private List<GraphDataPoint> GenerateCurrentMonthWeeklyData(List<Disbursement> disbursements, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var currentDate = monthStart;
        var weekNumber = 1;

        while (currentDate.Month == now.Month && currentDate <= now.Date)
        {
            var weekEnd = currentDate.AddDays(6);
            if (weekEnd.Month != now.Month || weekEnd > now.Date)
                weekEnd = now.Month == now.Month ? now.Date : new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            var weekDisbursements = disbursements.Where(d => d.CreatedAt.Date >= currentDate && d.CreatedAt.Date <= weekEnd);
            result.Add(new GraphDataPoint
            {
                Label = $"Week {weekNumber}",
                Value = weekDisbursements.Sum(d => d.Amount),
                Date = currentDate
            });

            currentDate = currentDate.AddDays(7);
            weekNumber++;
        }
        return result;
    }

    private List<GraphDataPoint> GenerateCurrentYearMonthlyData(List<Disbursement> disbursements, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int month = 1; month <= now.Month; month++)
        {
            var monthStart = new DateTime(now.Year, month, 1);
            var monthEnd = new DateTime(now.Year, month, DateTime.DaysInMonth(now.Year, month));
            if (month == now.Month)
                monthEnd = now.Date;

            var monthDisbursements = disbursements.Where(d => d.CreatedAt.Date >= monthStart && d.CreatedAt.Date <= monthEnd);
            result.Add(new GraphDataPoint
            {
                Label = monthStart.ToString("MMM"),
                Value = monthDisbursements.Sum(d => d.Amount),
                Date = monthStart
            });
        }
        return result;
    }

    // Loan request analytics methods (similar structure but for loan amounts)
    private List<GraphDataPoint> GenerateLast7DaysLoanData(List<Loan> loans, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            var dayLoans = loans.Where(l => l.CreatedAt.Date == date);
            result.Add(new GraphDataPoint
            {
                Label = date.ToString("MMM dd"),
                Value = dayLoans.Sum(l => l.Amount),
                Date = date
            });
        }
        return result;
    }

    private List<GraphDataPoint> GenerateCurrentMonthDailyLoanData(List<Loan> loans, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

        for (int day = 1; day <= Math.Min(daysInMonth, now.Day); day++)
        {
            var date = new DateTime(now.Year, now.Month, day);
            var dayLoans = loans.Where(l => l.CreatedAt.Date == date);
            result.Add(new GraphDataPoint
            {
                Label = date.ToString("dd"),
                Value = dayLoans.Sum(l => l.Amount),
                Date = date
            });
        }
        return result;
    }

    private List<GraphDataPoint> GenerateCurrentMonthWeeklyLoanData(List<Loan> loans, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var currentDate = monthStart;
        var weekNumber = 1;

        while (currentDate.Month == now.Month && currentDate <= now.Date)
        {
            var weekEnd = currentDate.AddDays(6);
            if (weekEnd.Month != now.Month || weekEnd > now.Date)
                weekEnd = now.Month == now.Month ? now.Date : new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));

            var weekLoans = loans.Where(l => l.CreatedAt.Date >= currentDate && l.CreatedAt.Date <= weekEnd);
            result.Add(new GraphDataPoint
            {
                Label = $"Week {weekNumber}",
                Value = weekLoans.Sum(l => l.Amount),
                Date = currentDate
            });

            currentDate = currentDate.AddDays(7);
            weekNumber++;
        }
        return result;
    }

    private List<GraphDataPoint> GenerateCurrentYearMonthlyLoanData(List<Loan> loans, DateTime now)
    {
        var result = new List<GraphDataPoint>();
        for (int month = 1; month <= now.Month; month++)
        {
            var monthStart = new DateTime(now.Year, month, 1);
            var monthEnd = new DateTime(now.Year, month, DateTime.DaysInMonth(now.Year, month));
            if (month == now.Month)
                monthEnd = now.Date;

            var monthLoans = loans.Where(l => l.CreatedAt.Date >= monthStart && l.CreatedAt.Date <= monthEnd);
            result.Add(new GraphDataPoint
            {
                Label = monthStart.ToString("MMM"),
                Value = monthLoans.Sum(l => l.Amount),
                Date = monthStart
            });
        }
        return result;
    }

    private async Task<FinancialMetrics> CalculateFinancialMetrics(Guid companyId, List<Loan> loans)
    {
        // Get all disbursements for the company
        var allLoans = await _loanRepository.GetAllLoansByCompanyId(companyId);
        var loanIds = allLoans.Select(l => l.Id.ToString()).ToList();
        
        var disbursements = new List<Disbursement>();
        var repayments = new List<Repayment>();
        
        foreach (var loanId in loanIds)
        {
            var loanDisbursements = await _disbursementRepository.GetDisbursementsByLoanId(loanId);
            disbursements.AddRange(loanDisbursements);
            
            var loanRepayments = await _repaymentRepository.GetRepaymentsByLoanId(loanId);
            repayments.AddRange(loanRepayments);
        }

        var totalDisbursed = disbursements.Sum(d => d.Amount);
        var totalRequested = loans.Sum(l => l.Amount);
        var totalRepaid = repayments.Sum(r => r.TotalRepaid);
        var outstandingBalance = totalDisbursed - totalRepaid;
        
        var approvedLoans = loans.Count(l => l.Status == LoanStatus.Approved);
        var defaultedLoans = loans.Count(l => l.Status == LoanStatus.Overdue);
        
        return new FinancialMetrics
        {
            TotalDisbursed = totalDisbursed,
            TotalRequested = totalRequested,
            AverageRequestAmount = loans.Count > 0 ? totalRequested / loans.Count : 0,
            AverageDisbursementAmount = disbursements.Count > 0 ? totalDisbursed / disbursements.Count : 0,
            OutstandingBalance = outstandingBalance,
            TotalRepaid = totalRepaid,
            RepaymentRate = totalDisbursed > 0 ? Math.Round((double)(totalRepaid / totalDisbursed) * 100, 2) : 0,
            DefaultedLoans = defaultedLoans,
            DefaultRate = approvedLoans > 0 ? Math.Round((double)defaultedLoans / approvedLoans * 100, 2) : 0
        };
    }

    private async Task<SystemMetrics> CalculateSystemMetrics(Guid companyId, List<ApplicationUser> users, List<Loan> loans)
    {
        // For now, we'll consider a user active if they've created a loan in the last 30 days
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentlyActiveUserIds = loans
            .Where(l => l.CreatedAt >= thirtyDaysAgo)
            .Select(l => l.UserId)
            .Distinct()
            .ToList();

        var activeUsers = users.Count(u => recentlyActiveUserIds.Contains(u.Id));
        var inactiveUsers = users.Count - activeUsers;

        var pendingLoans = loans.Count(l => l.Status == LoanStatus.Pending);
        var approvedLoans = loans.Count(l => l.Status == LoanStatus.Approved);
        var rejectedLoans = loans.Count(l => l.Status == LoanStatus.Rejected);
        var totalProcessedLoans = approvedLoans + rejectedLoans;

        // Get support tickets for the company
        var supportTickets = (await _supportTicketRepository.GetByCompanyIdAsync(companyId)).ToList();
        var openTickets = supportTickets.Count(t => t.Status?.ToLower() == "open" || t.Status?.ToLower() == "inprogress");
        var resolvedTickets = supportTickets.Count(t => t.Status?.ToLower() == "resolved" || t.Status?.ToLower() == "closed");

        return new SystemMetrics
        {
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            UserActivityRate = users.Count > 0 ? Math.Round((double)activeUsers / users.Count * 100, 2) : 0,
            PendingApprovals = pendingLoans,
            ApprovedLoans = approvedLoans,
            RejectedLoans = rejectedLoans,
            ApprovalRate = totalProcessedLoans > 0 ? Math.Round((double)approvedLoans / totalProcessedLoans * 100, 2) : 0,
            OpenSupportTickets = openTickets,
            TicketResolutionRate = supportTickets.Count > 0 ? Math.Round((double)resolvedTickets / supportTickets.Count * 100, 2) : 0
        };
    }
}
