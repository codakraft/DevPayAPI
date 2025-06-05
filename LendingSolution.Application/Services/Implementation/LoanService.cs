using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models.Response;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data; // Adjust namespace to your actual DbContext location
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;


namespace LendingSolution.Application.Services.Implementations;

public class LoanService : ILoanService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRemitaAuthService _remitaAuthService;
    private readonly IRemitaService _remitaService;

    public LoanService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IRemitaAuthService remitaAuthService, IRemitaService remitaService)
    {
        _db = db;
        _userManager = userManager;
        _remitaAuthService = remitaAuthService;
        _remitaService = remitaService;
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
            Status = "Pending",
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

        if (loan.Status == "Approved")
        {
            return new ApiResponse { Success = false, Message = "Loan already approved", Data = loan };
        }

        loan.Status = "Approved";
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
        loan.Status = "Submitted";
        _db.Loans.Update(loan);
        await _db.SaveChangesAsync();
        return new ApiResponse { Success = true, Message = "Loan submitted successfully", Data = null };
    }

    public async Task<ApiResponse<ReviewHistoryResponseDto>> SalaryHistoryReview(ReviewHistoryRequestDto body)
    {
        var remitaRequest = new
        {
            authorisationCode = "",
            firstName = "",
            lastName = "",
            middleName = "",
            accountNumber = body.AccountNumber,
            bankCode = body.BankCode,
            bvn = "",
            authorisationChannel = "USSD"
        };

        var apiKey = "REVNT01EQTEyMzR8REVNT01EQQ==";
        var merchantId = "DEMOMDA1234";
        var requestId = Guid.NewGuid().ToString();
        var authorization = await _remitaAuthService.GetAccessTokenAsync() ?? string.Empty;

        var remitaResponseJson = await _remitaService.GetSalaryHistoryAsync(remitaRequest, apiKey, merchantId, requestId, authorization);
        if (string.IsNullOrEmpty(remitaResponseJson))
        {
            return new ApiResponse<ReviewHistoryResponseDto> { Success = false, Message = "Failed to retrieve salary history from Remita", Data = null };
        }

        string companyName = string.Empty;
        decimal maxEligibleAmount = 0;
        try
        {
            using var doc = JsonDocument.Parse(remitaResponseJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("data", out var dataProp))
            {
                if (dataProp.TryGetProperty("companyName", out var companyNameProp))
                {
                    companyName = companyNameProp.GetString() ?? string.Empty;
                }
                if (dataProp.TryGetProperty("salaryPaymentDetails", out var salaryDetailsProp) && salaryDetailsProp.ValueKind == JsonValueKind.Array)
                {
                    var salaries = salaryDetailsProp.EnumerateArray()
                        .Select(x => decimal.TryParse(x.GetProperty("amount").GetString(), out var amt) ? amt : 0)
                        .Where(x => x > 0)
                        .ToList();
                    if (salaries.Count > 0)
                    {
                        var avgSalary = salaries.Average();
                        var minSalary = salaries.Min();
                        var maxByAvg = avgSalary * 2;
                        var maxByMin = minSalary * 3;
                        maxEligibleAmount = Math.Min(maxByAvg, maxByMin);
                    }
                }
            }
        }
        catch { }

        var response = new ReviewHistoryResponseDto
        {
            CompanyName = companyName,
            MaxEligibleAmount = maxEligibleAmount
        };
        return new ApiResponse<ReviewHistoryResponseDto>
        {
            Data = response,
            Success = true,
            Message = "Salary history reviewed successfully"
        };
    }
}