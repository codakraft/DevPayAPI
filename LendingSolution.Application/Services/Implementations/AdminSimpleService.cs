using System.Threading.Tasks;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Services.Implementations
{
    public class AdminSimpleService : IAdminSimpleService
    {
        private readonly ApplicationDbContext _db;
        public AdminSimpleService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserSimpleAdminDto> GetUseAsync()
        {
            // Example: get the first user (customize as needed)
            var user = await _db.Users.FirstOrDefaultAsync();
            if (user == null) return null;
            return new UserSimpleAdminDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Email = user.Email,
                Gender = user.Gender,
                IsActive = user.IsActive
            };
        }
    }
}
