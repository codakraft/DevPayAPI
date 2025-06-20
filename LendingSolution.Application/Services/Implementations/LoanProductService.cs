using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class LoanProductService : ILoanProductService
    {
        private readonly ApplicationDbContext _db;
        public LoanProductService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<LoanProductDto>> GetLoanProductsAsync(LoanProductFilterDto filter)
        {
            var query = _db.LoanProducts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(p => p.Status == filter.Status);
            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(p => p.Name.Contains(filter.Name));
            return await query.Select(p => new LoanProductDto
            {
                Id = p.Id,
                Name = p.Name,
                InterestRate = p.InterestRate,
                MinAmount = p.MinAmount,
                MaxAmount = p.MaxAmount,
                MinTenure = p.MinTenure,
                MaxTenure = p.MaxTenure,
                Status = p.Status,
                Moratorium = p.Moratorium
            }).ToListAsync();
        }

        public async Task<LoanProductDto> GetLoanProductByIdAsync(string id)
        {
            var p = await _db.LoanProducts.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return null;
            return new LoanProductDto
            {
                Id = p.Id,
                Name = p.Name,
                InterestRate = p.InterestRate,
                MinAmount = p.MinAmount,
                MaxAmount = p.MaxAmount,
                MinTenure = p.MinTenure,
                MaxTenure = p.MaxTenure,
                Status = p.Status,
                Moratorium = p.Moratorium
            };
        }

        public async Task<bool> DeactivateLoanProductAsync(string id)
        {
            var p = await _db.LoanProducts.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null || p.Status == "Inactive") return false;
            p.Status = "Inactive";
            _db.LoanProducts.Update(p);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateLoanProductAsync(string id)
        {
            var p = await _db.LoanProducts.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null || p.Status == "Active") return false;
            p.Status = "Active";
            _db.LoanProducts.Update(p);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteLoanProductAsync(string id)
        {
            var p = await _db.LoanProducts.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return false;
            _db.LoanProducts.Remove(p);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateLoanProductAsync(string id, LoanProductUpdateDto update)
        {
            var p = await _db.LoanProducts.FirstOrDefaultAsync(x => x.Id == id);
            if (p == null) return false;
            if (!string.IsNullOrWhiteSpace(update.Name)) p.Name = update.Name;
            if (update.InterestRate.HasValue) p.InterestRate = update.InterestRate.Value;
            if (update.MinAmount.HasValue) p.MinAmount = update.MinAmount.Value;
            if (update.MaxAmount.HasValue) p.MaxAmount = update.MaxAmount.Value;
            if (update.MinTenure.HasValue) p.MinTenure = update.MinTenure.Value;
            if (update.MaxTenure.HasValue) p.MaxTenure = update.MaxTenure.Value;
            if (!string.IsNullOrWhiteSpace(update.Status)) p.Status = update.Status;
            if (update.Moratorium.HasValue) p.Moratorium = update.Moratorium.Value;
            _db.LoanProducts.Update(p);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
