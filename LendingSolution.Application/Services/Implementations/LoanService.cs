using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data; // Adjust namespace to your actual DbContext location
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Dtos.Response;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Nodes;


namespace LendingSolution.Application.Services.Implementations;

public class LoanService : ILoanService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRemitaService _remitaService;
    private readonly IConfiguration _configuration;

    public LoanService(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IRemitaService remitaService,
        IConfiguration configuration
)
    {
        _db = db;
        _userManager = userManager;
        _remitaService = remitaService;
        _configuration = configuration;
    }

    public async Task<ApiResponse> ApplyForLoan(LoanApplicationDto dto, ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            return new ApiResponse { Success = false, Message = "User not found", Data = null };
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

        return new ApiResponse { Success = true, Message = "Loan application submitted", Data = loan };
    }

    public async Task<ApiResponse> GetUserLoans(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new ApiResponse { Success = false, Message = "User ID is required", Data = null };
        }
        var loans = await _db.Loans.Where(l => l.UserId == userId).ToListAsync();
        return new ApiResponse { Success = true, Message = "User loans fetched successfully", Data = loans };
    }

    public async Task<ApiResponse> ApproveLoan(Guid loanId)
    {
        var loan = await _db.Loans.FindAsync(loanId);
        if (loan == null)
        {
            return new ApiResponse { Success = false, Message = "Loan not found", Data = null };
        }

        if (loan.Status == LoanStatus.Approved)
        {
            return new ApiResponse { Success = false, Message = "Loan already approved", Data = loan };
        }

        loan.Status = LoanStatus.Approved;
        loan.ApprovedAt = DateTime.UtcNow;
        loan.DueDate = DateTime.UtcNow.AddMonths(loan.DurationInMonths);

        _db.Loans.Update(loan);
        await _db.SaveChangesAsync();

        return new ApiResponse { Success = true, Message = "Loan approved", Data = loan };
    }

    public async Task<ApiResponse> GetAllLoans()
    {
        var loans = await _db.Loans
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return new ApiResponse { Success = true, Message = "All loans fetched", Data = loans };
    }

    public ApiResponse GetLoanBreakdown(LoanBreakdownRequestDto body)
    {
        // Basic checks
        if (body.Amount <= 0 || body.DurationInMonths <= 0)
        {
            return new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Amount and duration must be greater than zero."
            };
        }

        // Simple equal repayment calculation
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
            Data = response,
            Success = true,
            Message = "Loan breakdown generated successfully"
        };
    }

    public async Task<ApiResponse> SubmitLoan(Guid loanId, System.Security.Claims.ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrEmpty(userId))
        {
            return new ApiResponse { Success = false, Message = "User not found", Data = null };
        }
        var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == loanId && l.UserId == userId);
        if (loan == null)
        {
            return new ApiResponse { Success = false, Message = "Loan not found for user", Data = null };
        }
        loan.Status = LoanStatus.Pending;
        _db.Loans.Update(loan);
        await _db.SaveChangesAsync();
        return new ApiResponse { Success = true, Message = "Loan submitted successfully", Data = null };
    }

    public async Task<ApiResponse<ReviewHistoryResponseDto>> SalaryHistoryReview(ReviewHistoryRequestDto body)
    {
        var loanInfo = await _db.Loans
            .Where(l => l.Id == body.loanId && l.Status == LoanStatus.NotBooked) // only unbooked loans
            .Include(l => l.User)
            .OrderByDescending(l => l.CreatedAt)
        .FirstOrDefaultAsync();

        if (loanInfo is null)
        {
            return new ApiResponse<ReviewHistoryResponseDto>
            {
                Success = false,
                Message = "Loan not found or already booked",
                Data = null
            };
        }

        var account = await _db.Account.FirstOrDefaultAsync(a => a.UserId == loanInfo.UserId);
        if (account is null)
        {
            return new ApiResponse<ReviewHistoryResponseDto>
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

        var salaryResponse = await _remitaService.GetSalaryHistoryAsync(remitaRequest);

        if (salaryResponse == null)
        {
            return new ApiResponse<ReviewHistoryResponseDto>
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
            return new ApiResponse<ReviewHistoryResponseDto>
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

        return new ApiResponse<ReviewHistoryResponseDto>
        {
            Data = response,
            Success = true,
            Message = "Salary history reviewed successfully"
        };
    }
}