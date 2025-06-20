using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;
using LendingSolution.Core.Models;
using LendingSolution.Core.Models.Response;
using LendingSolution.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace LendingSolution.Application.Services.Implementations;

public class RepaymentService : IRepaymentService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public RepaymentService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ApiResponse> MakeRepayment(RepaymentDto dto, ClaimsPrincipal user)
    {
        var userId = _userManager.GetUserId(user);
        if (userId == null)
        {
            return new ApiResponse { Success = false, Message = "User not found", Data = null };
        }

        var loan = await _db.Loans.FindAsync(dto.LoanId);
        if (loan == null || loan.UserId != userId)
        {
            return new ApiResponse { Success = false, Message = "Loan not found or access denied", Data = null };
        }

        if (dto.Amount <= 0)
        {
            return new ApiResponse { Success = false, Message = "Repayment amount must be positive", Data = null };
        }

        // Calculate total repaid so far
        var totalRepaid = _db.Repayments
            .Where(r => r.LoanId == loan.Id)
            .Sum(r => (decimal?)r.Amount) ?? 0m;

        var outstanding = loan.Amount - totalRepaid;
        if (dto.Amount > outstanding)
        {
            return new ApiResponse { Success = false, Message = "Repayment exceeds outstanding balance", Data = null };
        }

        var repayment = new Repayment
        {
            Id = Guid.NewGuid(),
            LoanId = loan.Id,
            Amount = dto.Amount,
            PaidAt = DateTime.UtcNow
        };

        _db.Repayments.Add(repayment);

        // If fully repaid, update loan status
        if (dto.Amount == outstanding)
        {
            loan.Status = LoanStatus.Repaid;
            _db.Loans.Update(loan);
        }

        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Success = true,
            Message = "Repayment successful",
            Data = new
            {
                Repayment = repayment,
                Outstanding = outstanding - dto.Amount,
                LoanStatus = loan.Status
            }
        };
    }
}