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
    public async Task<ApiResponse> Register(RegisterRequestDto body)
    {
        var existingCompany = await _companyRepository.GetCompanyById(body.CompanyId);
        if (existingCompany is null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Company not found",
                Data = null
            };
        }

        var existingEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == body.Email);

        if (existingEmail != null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "A user with this email already exists.",
                Data = null
            };
        }

        var existingBvn = await _db.Account.FirstOrDefaultAsync(u => u.Bvn == body.Bvn);

        if (existingBvn != null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Bvn already exists.",
                Data = null
            };
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
            return new ApiResponse
            {
                Success = false,
                Message = "User creation failed",
                Data = result.Errors
            };
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
            return new ApiResponse
            {
                Success = false,
                Message = "Loan creation failed",
                Data = null
            };
        }

        return new ApiResponse
        {
            Success = true,
            Data = loan.Id,
            Message = "User created successfully"
        };
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

    public async Task<ApiResponse> GetAllLoans()
    {
        var loans = await _db.Loans
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return new ApiResponse
        {
            Success = true,
            Message = "All loans fetched",
            Data = loans
        };
    }
    // under construction
    public ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body)
    {
        if (body.Amount <= 0 || body.DurationInMonths <= 0)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Amount and duration must be greater than zero.",
                Data = null
            };
        }

        var monthlyRepayment = Math.Round(body.Amount / body.DurationInMonths, 2);
        var schedules = new List<RepaymentScheduleDto>();
        var today = DateTime.UtcNow.Date;

        for (int i = 1; i <= body.DurationInMonths; i++)
        {
            schedules.Add(new RepaymentScheduleDto
            {
                RepaymentDate = today.AddMonths(i),
                Amount = monthlyRepayment
            });
        }

        var response = new LoanBreakdownResponseDto
        {
            LoanAmount = body.Amount,
            Tenor = body.DurationInMonths,
            NextRepaymentDate = schedules[0].RepaymentDate,
            RepaymentSchedules = schedules
        };

        return new ApiResponse
        {
            Success = true,
            Message = "Loan breakdown generated successfully",
            Data = response
        };
    }

    public async Task<ApiResponse> SubmitLoan(Guid loanId, SubmitRequestDto body)
    {
        var loan = await _loanRepository.GetLoanById(loanId);

        if (loan == null)
        {
            return ApiResponse.Fail
            (
                "Loan not found for user"
            );
        }

        var generateMandate = await _remitaService.GenerateMandate(loanId, body);

        loan.Status = LoanStatus.Pending;
        loan.IsMandateGenerated = true;

        var saveLoan = await _loanRepository.UpdateLoan(loan);

        if (saveLoan)
            ApiResponse.Fail("Failed to update loan status");

        return ApiResponse.Ok("Loan booked successfully. You will be notified of status of your loan.");
    }

    public async Task<ApiResponse> SalaryHistoryReview(ReviewHistoryRequestDto body)
    {
        var loanInfo = await _db.Loans
            .Where(l => l.Id == body.loanId && l.Status == LoanStatus.NotBooked)
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync();

        if (loanInfo is null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Loan not found or already booked",
                Data = null
            };
        }

        var account = await _db.Account.FirstOrDefaultAsync(a => a.UserId == loanInfo.UserId);
        if (account is null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Account not found for user",
                Data = null
            };
        }

        var remitaRequest = new
        {
            authorisationCode = "1234",
            firstName = loanInfo.User.FirstName ?? "",
            lastName = loanInfo.User.LastName ?? "",
            middleName = string.Empty,
            accountNumber = body.AccountNumber,
            bankCode = body.BankCode,
            bvn = account.Bvn ?? "",
            authorisationChannel = "USSD"
        };

        var salaryResponse = await _remitaService.GetSalaryHistory(remitaRequest);

        if (salaryResponse == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Failed to retrieve salary history",
                Data = null
            };
        }

        account.BankCode = body.BankCode;
        account.AccountNumber = body.AccountNumber;
        account.AccountName = salaryResponse.Data?.CustomerName ?? string.Empty;

        await _db.SaveChangesAsync();

        var product = await _db.LoanProducts.FirstOrDefaultAsync(p => p.Id == loanInfo.ProductId);
        if (product is null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Loan product not found",
                Data = null
            };
        }

        var response = new ReviewHistoryResponseDto
        {
            CompanyName = salaryResponse.Data?.CompanyName ?? string.Empty,
            MaxEligibleAmount = product.MaxAmount
        };

        return new ApiResponse
        {
            Success = true,
            Message = "Salary history reviewed successfully",
            Data = response
        };
    }
}
