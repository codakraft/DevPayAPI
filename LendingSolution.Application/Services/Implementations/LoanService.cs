using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Dtos.Response;
using Microsoft.Extensions.Configuration;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class LoanService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager,
    IRemitaService remitaService,
    ILoanRepository loanRepository,
    ICompanyRepository companyRepository,
    IConfiguration configuration
) : ILoanService
{
    private readonly ApplicationDbContext _db = db;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;

    public async Task<String> Register(RegisterRequestDto body)
    {
        var existingCompany = await _companyRepository.GetCompanyById(body.CompanyId);
        if (existingCompany is null)
        {
            throw new AppException("Company not found", 404);
        }

        var existingEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == body.Email);

        if (existingEmail != null)
        {
            throw new AppException("A user with this email already exists");
        }

        var existingBvn = await _db.BorrowerApplications.FirstOrDefaultAsync(u => u.BVN == body.Bvn);

        if (existingBvn != null)
        {
            throw new AppException("A user with this BVN already exists");
        }

        var user = new ApplicationUser
        {
            UserName = body.Email,
            Email = body.Email,
            FirstName = string.Empty,
            LastName = string.Empty,
            Address = string.Empty,
            City = string.Empty,
            State = string.Empty,
            DateOfBirth = body.DateOfBirth,
            PhoneNumber = body.PhoneNumber,
        };

        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            throw new AppException(
                "User creation failed: " + string.Join(", ", result.Errors.Select(e => e.Description))
            );
        }

        // add a new loan
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Amount = 0,
            DurationInMonths = 0,
            Purpose = string.Empty,
            Status = LoanStatus.NotBooked,
            CompanyId = body.CompanyId
        };

        var loanResult = await _loanRepository.CreateLoan(loan);

        if (loanResult is false)
        {
            throw new AppException("Loan creation failed");
        }

        return loan.Id.ToString();
    }

    public async Task<Loan> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            throw new AppException("User not found", 404);
        }

        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = dto.Amount,
            DurationInMonths = dto.DurationInMonths,
            Purpose = dto.Purpose,
            Status = LoanStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.Loans.Add(loan);
        await _db.SaveChangesAsync();

        return loan;
    }

    public async Task<Loan> ApproveLoan(Guid loanId)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        if (loan.Status == LoanStatus.Approved)
        {
            throw new AppException("Loan already approved", 400);
        }

        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        loan.DueDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths);

        _db.Loans.Update(loan);
        await _db.SaveChangesAsync();

        return loan;
    }

    public async Task<List<Loan>> GetAllLoans()
    {
        return await _db.Loans
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<PagedLoanListDto> GetAllLoansAsync(LoanFilterDto filter)
    {
        var query = _loanRepository.GetAllLoansQueryable();

        // Apply filters
        query = ApplyFilters(query, filter);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Calculate summary statistics
        var totalAmount = await query.SumAsync(l => l.Amount);
        var avgAmount = totalCount > 0 ? totalAmount / totalCount : 0;
        var statusCounts = await query
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        // Apply sorting
        query = ApplySorting(query, filter);

        // Apply pagination
        var pagedLoans = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Map to DTOs
        var loanDtos = pagedLoans.Select(MapToLoanListDto).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedLoanListDto
        {
            Loans = loanDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1,
            TotalLoanAmount = totalAmount,
            AverageAmount = avgAmount,
            StatusCounts = statusCounts
        };
    }

    public async Task<PagedLoanListDto> GetCompanyLoansAsync(Guid companyId, LoanFilterDto filter)
    {
        var query = _loanRepository.GetCompanyLoansQueryable(companyId);

        // Apply filters (excluding company filter since it's already filtered)
        query = ApplyFilters(query, filter, excludeCompanyFilter: true);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Calculate summary statistics
        var totalAmount = await query.SumAsync(l => l.Amount);
        var avgAmount = totalCount > 0 ? totalAmount / totalCount : 0;
        var statusCounts = await query
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());

        // Apply sorting
        query = ApplySorting(query, filter);

        // Apply pagination
        var pagedLoans = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        // Map to DTOs
        var loanDtos = pagedLoans.Select(MapToLoanListDto).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

        return new PagedLoanListDto
        {
            Loans = loanDtos,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            HasNextPage = filter.Page < totalPages,
            HasPreviousPage = filter.Page > 1,
            TotalLoanAmount = totalAmount,
            AverageAmount = avgAmount,
            StatusCounts = statusCounts
        };
    }

    public async Task<LoanListDto> GetLoanByIdAsync(Guid loanId, string? requestingUserId = null)
    {
        var loan = await _db.Loans
            .Include(l => l.User)
            .Include(l => l.Company)
            .Include(l => l.Product)
            .Include(l => l.BorrowerApplication)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
        {
            throw new AppException("Loan not found", 404);
        }

        // Optional: Add access control if requestingUserId is provided
        // This can be enhanced based on business rules

        return MapToLoanListDto(loan);
    }

    private IQueryable<Loan> ApplyFilters(IQueryable<Loan> query, LoanFilterDto filter, bool excludeCompanyFilter = false)
    {
        // Search filter (user name, email, purpose)
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchTerm = filter.Search.ToLower();
            query = query.Where(l =>
                (l.User != null && (
                    l.User.FirstName.ToLower().Contains(searchTerm) ||
                    l.User.LastName.ToLower().Contains(searchTerm) ||
                    (l.User.Email != null && l.User.Email.ToLower().Contains(searchTerm))
                )) ||
                (l.BorrowerApplication != null && (
                    l.BorrowerApplication.FirstName.ToLower().Contains(searchTerm) ||
                    l.BorrowerApplication.LastName.ToLower().Contains(searchTerm) ||
                    l.BorrowerApplication.Email.ToLower().Contains(searchTerm)
                )) ||
                l.Purpose.ToLower().Contains(searchTerm));
        }

        // Company filter (only for SuperAdmin view)
        if (!excludeCompanyFilter && filter.CompanyId.HasValue)
        {
            query = query.Where(l => l.CompanyId == filter.CompanyId.Value);
        }

        // Status filter
        if (filter.Status.HasValue)
        {
            query = query.Where(l => l.Status == filter.Status.Value);
        }

        // Amount range filter
        if (filter.MinAmount.HasValue)
        {
            query = query.Where(l => l.Amount >= filter.MinAmount.Value);
        }
        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(l => l.Amount <= filter.MaxAmount.Value);
        }

        // Duration range filter
        if (filter.MinDuration.HasValue)
        {
            query = query.Where(l => l.DurationInMonths >= filter.MinDuration.Value);
        }
        if (filter.MaxDuration.HasValue)
        {
            query = query.Where(l => l.DurationInMonths <= filter.MaxDuration.Value);
        }

        // Date range filters
        if (filter.StartDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= filter.StartDate.Value);
        }
        if (filter.EndDate.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= filter.EndDate.Value);
        }

        // Approval date filters
        if (filter.ApprovedAfter.HasValue)
        {
            query = query.Where(l => l.ApprovedAt >= filter.ApprovedAfter.Value);
        }
        if (filter.ApprovedBefore.HasValue)
        {
            query = query.Where(l => l.ApprovedAt <= filter.ApprovedBefore.Value);
        }

        // Product filter
        if (filter.ProductId.HasValue)
        {
            query = query.Where(l => l.ProductId == filter.ProductId.Value);
        }

        // Mandate filter
        if (filter.IsMandateGenerated.HasValue)
        {
            query = query.Where(l => l.IsMandateGenerated == filter.IsMandateGenerated.Value);
        }

        return query;
    }

    private IQueryable<Loan> ApplySorting(IQueryable<Loan> query, LoanFilterDto filter)
    {
        return filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.Amount)
                : query.OrderBy(l => l.Amount),
            "duration" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.DurationInMonths)
                : query.OrderBy(l => l.DurationInMonths),
            "status" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.Status)
                : query.OrderBy(l => l.Status),
            "username" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.User != null ? l.User.FirstName : l.BorrowerApplication != null ? l.BorrowerApplication.FirstName : string.Empty)
                       .ThenByDescending(l => l.User != null ? l.User.LastName : l.BorrowerApplication != null ? l.BorrowerApplication.LastName : string.Empty)
                : query.OrderBy(l => l.User != null ? l.User.FirstName : l.BorrowerApplication != null ? l.BorrowerApplication.FirstName : string.Empty)
                       .ThenBy(l => l.User != null ? l.User.LastName : l.BorrowerApplication != null ? l.BorrowerApplication.LastName : string.Empty),
            "companyname" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.Company.Name)
                : query.OrderBy(l => l.Company.Name),
            "approvedat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.ApprovedAt)
                : query.OrderBy(l => l.ApprovedAt),
            "duedate" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.DueDate)
                : query.OrderBy(l => l.DueDate),
            "updatedat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.UpdatedAt)
                : query.OrderBy(l => l.UpdatedAt),
            "createdat" => filter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(l => l.CreatedAt)
                : query.OrderBy(l => l.CreatedAt),
            _ => query.OrderByDescending(l => l.CreatedAt)
        };
    }

    private LoanListDto MapToLoanListDto(Loan loan)
    {
        // Get user information from User or BorrowerApplication
        string firstName;
        string lastName;
        string email;
        
        if (loan.User != null)
        {
            firstName = loan.User.FirstName;
            lastName = loan.User.LastName;
            email = loan.User.Email ?? string.Empty;
        }
        else if (loan.BorrowerApplication != null)
        {
            firstName = loan.BorrowerApplication.FirstName;
            lastName = loan.BorrowerApplication.LastName;
            email = loan.BorrowerApplication.Email;
        }
        else
        {
            // Fallback - this should rarely happen
            firstName = "Unknown";
            lastName = "User";
            email = string.Empty;
        }
        
        return new LoanListDto
        {
            Id = loan.Id,
            UserId = loan.UserId ?? string.Empty,
            UserFirstName = firstName,
            UserLastName = lastName,
            UserEmail = email,
            Amount = loan.Amount,
            DurationInMonths = loan.DurationInMonths,
            Purpose = loan.Purpose,
            Status = loan.Status,
            ApprovedAt = loan.ApprovedAt,
            DueDate = loan.DueDate,
            RejectedAt = loan.RejectedAt,
            CreatedAt = loan.CreatedAt,
            UpdatedAt = loan.UpdatedAt,
            CompanyId = loan.CompanyId,
            CompanyName = loan.Company.Name,
            CompanyShortName = loan.Company.ShortName,
            ProductId = loan.ProductId,
            ProductName = loan.Product.Name,
            ProductInterestRate = loan.Product.InterestRate,
            Message = loan.Message,
            IsMandateGenerated = loan.IsMandateGenerated,
            MandateId = loan.MandateId
        };
    }
}

    // ...existing code...
