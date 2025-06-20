using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _db;
        public AdminService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserAdminDto>> GetUsersAsync(UserFilterDto filter)
        {
            var query = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(filter.Role))
                query = query.Where(u => u.Role == filter.Role);
            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);
            if (!string.IsNullOrWhiteSpace(filter.Email))
                query = query.Where(u => u.Email == filter.Email);
            if (!string.IsNullOrWhiteSpace(filter.Gender))
                query = query.Where(u => u.Gender == filter.Gender);

            return await query.Select(u => new UserAdminDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                Email = u.Email,
                Gender = u.Gender,
                IsActive = u.IsActive
            }).ToListAsync();
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || !user.IsActive)
                return false;
            user.IsActive = false;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.IsActive)
                return false;
            user.IsActive = true;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
