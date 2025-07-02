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

        var existingBvn = await _db.Account.FirstOrDefaultAsync(u => u.Bvn == body.Bvn);

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

    public async Task<ApiResponse> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "User not found",
                Data = null
            };
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

        return new ApiResponse
        {
            Success = true,
            Message = "Loan application submitted",
            Data = loan
        };
    }

    public async Task<ApiResponse> GetUserLoans(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new ApiResponse
            {
                Success = false,
                Message = "User ID is required",
                Data = null
            };
        }

        var loans = await _db.Loans
            .Where(l => l.UserId == userId)
            .ToListAsync();

        return new ApiResponse
        {
            Success = true,
            Message = "User loans fetched successfully",
            Data = loans
        };
    }

    public async Task<ApiResponse> ApproveLoan(Guid loanId)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Loan not found",
                Data = null
            };
        }

        if (loan.Status == LoanStatus.Approved)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Loan already approved",
                Data = loan
            };
        }

        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        loan.DueDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths);

        _db.Loans.Update(loan);
        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Success = true,
            Message = "Loan approved",
            Data = loan
        };
    }

    public async Task<List<Loan>> GetAllLoans()
    {
        return await _db.Loans
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

    }


}
