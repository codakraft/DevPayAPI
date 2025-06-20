using System;
using System.Linq;
using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _db;
        public DashboardService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var yearStart = new DateTime(now.Year, 1, 1);

            var repayments = _db.Repayments.AsQueryable();
            var loans = _db.Loans.AsQueryable();

            decimal GetRepaid(Func<DateTime, bool> dateFilter) =>
                repayments.Where(r => dateFilter(r.PaidAt)).Sum(r => (decimal?)r.Amount) ?? 0m;
            decimal GetDisbursed(Func<DateTime, bool> dateFilter) =>
                loans.Where(l => l.Status == Core.Enum.LoanStatus.Disbursed && l.CreatedAt != null && dateFilter(l.CreatedAt)).Sum(l => (decimal?)l.Amount) ?? 0m;

            var data = new DashboardDataDto
            {
                today = new PeriodData
                {
                    totalRepaid = GetRepaid(d => d.Date == today),
                    totalDisbursed = GetDisbursed(d => d.Date == today)
                },
                thisMonth = new PeriodData
                {
                    totalRepaid = GetRepaid(d => d >= monthStart),
                    totalDisbursed = GetDisbursed(d => d >= monthStart)
                },
                thisYear = new PeriodData
                {
                    totalRepaid = GetRepaid(d => d >= yearStart),
                    totalDisbursed = GetDisbursed(d => d >= yearStart)
                },
                overall = new PeriodData
                {
                    totalRepaid = GetRepaid(d => true),
                    totalDisbursed = GetDisbursed(d => true)
                }
            };
            return data;
        }
    }
}
