using Microsoft.EntityFrameworkCore;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Enum;

namespace LendingSolution.Application.Services.Implementations;

public class AdminLoanService : IAdminLoanService
{
    private readonly ApplicationDbContext _db;
    public AdminLoanService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<LoanRequestDto>> GetLoanRequestsAsync(LoanRequestFilterDto filter)
    {
        var query = _db.LoanRequests.AsQueryable();
        if (filter.StartDate.HasValue)
            query = query.Where(r => r.RequestDate >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            query = query.Where(r => r.RequestDate <= filter.EndDate.Value);
        if (!string.IsNullOrWhiteSpace(filter.Email))
            query = query.Where(r => r.Email.Contains(filter.Email));
        if (!string.IsNullOrWhiteSpace(filter.FirstName))
            query = query.Where(r => r.FirstName.Contains(filter.FirstName));
        if (!string.IsNullOrWhiteSpace(filter.LastName))
            query = query.Where(r => r.LastName.Contains(filter.LastName));

        return await query.Select(r => new LoanRequestDto
        {
            Id = r.Id,
            Bvn = r.Bvn,
            Email = r.Email,
            FirstName = r.FirstName,
            LastName = r.LastName,
            SalaryBank = r.SalaryBank,
            AccountNumber = r.AccountNumber,
            AmountRequested = r.AmountRequested,
            DurationInMonths = r.DurationInMonths,
            Status = r.Status,
            Message = r.Message
        }).ToListAsync();
    }

    public async Task<LoanRequestDto> GetLoanRequestByIdAsync(Guid id)
    {
        var r = await _db.Loans
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return null;

        return new LoanRequestDto
        {
            Id = r.Id,
            Bvn = r.Bvn,
            Email = r.Email,
            FirstName = r.FirstName,
            LastName = r.LastName,
            SalaryBank = r.SalaryBank,
            AccountNumber = r.AccountNumber,
            AmountRequested = r.AmountRequested,
            DurationInMonths = r.DurationInMonths,
            Status = r.Status.ToString,
            Message = r.Message
        };
    }

    public async Task<bool> ApproveLoanRequestAsync(string id)
    {
        var r = await _db.LoanRequests.FirstOrDefaultAsync(x => x.Id == id);
        if (r == null || r.Status == "Approved") return false;
        r.Status = "Approved";
        _db.LoanRequests.Update(r);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RejectLoanRequestAsync(Guid id, string message = null)
    {
        var r = await _db.Loans.FirstOrDefaultAsync(x => x.Id == id);
        if (r == null || r.Status == LoanStatus.Rejected) return false;
        r.Status =  LoanStatus.Rejected;
        if (!string.IsNullOrWhiteSpace(message)) r.Message = message;
        _db.Loans.Update(r);
        await _db.SaveChangesAsync();
        return true;
    }
}
