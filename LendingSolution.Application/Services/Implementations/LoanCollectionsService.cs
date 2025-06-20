using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class LoanCollectionsService : ILoanCollectionsService
    {
        private readonly ApplicationDbContext _db;
        public LoanCollectionsService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Loan Requests (reuse logic from AdminLoanService if needed)
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

        public async Task<LoanRequestDto> GetLoanRequestByIdAsync(string id)
        {
            var r = await _db.LoanRequests.FirstOrDefaultAsync(x => x.Id == id);
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
                Status = r.Status,
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

        public async Task<bool> RejectLoanRequestAsync(string id, string message = null)
        {
            var r = await _db.LoanRequests.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null || r.Status == "Rejected") return false;
            r.Status = "Rejected";
            if (!string.IsNullOrWhiteSpace(message)) r.Message = message;
            _db.LoanRequests.Update(r);
            await _db.SaveChangesAsync();
            return true;
        }

        // Unpaid Loans
        public async Task<IEnumerable<UnpaidLoanDto>> GetUnpaidLoansAsync()
        {
            return await _db.Loans.Where(l => l.Status == "Overdue" || l.Status == "Pending")
                .Select(l => new UnpaidLoanDto
                {
                    Id = l.Id,
                    Name = l.User.FirstName + " " + l.User.LastName,
                    AccountNo = l.User.AccountNumber,
                    LoanAmount = l.Amount,
                    RequestDate = l.CreatedAt,
                    TenorInMonths = l.DurationInMonths,
                    UnpaidPrincipal = l.Amount - l.Repayments.Sum(r => r.Amount),
                    UnpaidInterest = 0, // Calculate as needed
                    TotalUnpaid = (l.Amount - l.Repayments.Sum(r => r.Amount)) // + interest
                }).ToListAsync();
        }

        // Ongoing Collections
        public async Task<IEnumerable<OngoingCollectionDto>> GetOngoingCollectionsAsync()
        {
            return await _db.Loans.Where(l => l.Status == "Disbursed")
                .Select(l => new OngoingCollectionDto
                {
                    Id = l.Id,
                    Name = l.User.FirstName + " " + l.User.LastName,
                    AccountNo = l.User.AccountNumber,
                    LoanAmount = l.Amount,
                    RequestDate = l.CreatedAt,
                    TenorInMonths = l.DurationInMonths,
                    UnpaidPrincipal = l.Amount - l.Repayments.Sum(r => r.Amount),
                    UnpaidInterest = 0, // Calculate as needed
                    TotalUnpaid = (l.Amount - l.Repayments.Sum(r => r.Amount)) // + interest
                }).ToListAsync();
        }

        // Upcoming Collections
        public async Task<IEnumerable<UpcomingCollectionDto>> GetUpcomingCollectionsAsync(DateTime? dueDate = null)
        {
            var query = _db.Loans.AsQueryable();
            if (dueDate.HasValue)
                query = query.Where(l => l.DueDate == dueDate.Value);
            return await query.Select(l => new UpcomingCollectionDto
            {
                Id = l.Id,
                FirstName = l.User.FirstName,
                LastName = l.User.LastName,
                AccountNumber = l.User.AccountNumber,
                DueDate = l.DueDate ?? DateTime.MinValue,
                Principal = l.Amount, // or next due principal
                Interest = 0, // Calculate as needed
                Fee = 0, // Calculate as needed
                Total = l.Amount // + interest + fee
            }).ToListAsync();
        }

        // Successful Repayments
        public async Task<IEnumerable<SuccessfulRepaymentDto>> GetSuccessfulRepaymentsAsync(DateTime? transactionDate = null)
        {
            var query = _db.Repayments.AsQueryable();
            if (transactionDate.HasValue)
                query = query.Where(r => r.PaidAt.Date == transactionDate.Value.Date);
            return await query.Select(r => new SuccessfulRepaymentDto
            {
                Id = r.Id,
                LoanId = r.LoanId,
                TransactionDate = r.PaidAt,
                FirstName = r.Loan.User.FirstName,
                LastName = r.Loan.User.LastName,
                AccountNumber = r.Loan.User.AccountNumber,
                Amount = r.Amount,
                LoanAmount = r.Loan.Amount,
                Tenor = r.Loan.DurationInMonths,
                Fee = 0, // Calculate as needed
                Total = r.Amount // + fee
            }).ToListAsync();
        }
    }
}
