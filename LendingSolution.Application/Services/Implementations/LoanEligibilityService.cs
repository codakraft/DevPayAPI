using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class LoanEligibilityService : ILoanEligibilityService
    {
        private readonly ApplicationDbContext _db;
        public LoanEligibilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<LoanEligibilityDto>> GetLoanEligibilityListAsync(LoanEligibilityFilterDto filter)
        {
            var query = _db.LoanEligibilities.AsQueryable();
            if (filter.StartDate.HasValue)
                query = query.Where(e => e.RequestDate >= filter.StartDate.Value);
            if (filter.EndDate.HasValue)
                query = query.Where(e => e.RequestDate <= filter.EndDate.Value);

            return await query.Select(e => new LoanEligibilityDto
            {
                RequestDate = e.RequestDate,
                Bvn = e.Bvn,
                Email = e.Email,
                SalaryBank = e.SalaryBank,
                AccountNumber = e.AccountNumber,
                AmountRequested = e.AmountRequested,
                DurationInMonths = e.DurationInMonths,
                Status = e.Status,
                Message = e.Message
            }).ToListAsync();
        }
    }
}
