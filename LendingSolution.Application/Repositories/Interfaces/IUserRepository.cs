using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<List<ApplicationUser>> GetAllUsersAsync();
    Task<List<ApplicationUser>> GetUsersInRoleAsync(string role);
    Task<List<ApplicationUser>> GetUsersByCompanyIdAsync(string companyId);
    Task<List<ApplicationUser>> GetActiveUsersAsync();
    Task<List<ApplicationUser>> GetUsersByGenderAsync(string gender);
    Task<List<ApplicationUser>> GetUsersCreatedAfterAsync(DateTime date);
    Task<List<ApplicationUser>> GetUsersCreatedBetweenAsync(DateTime startDate, DateTime endDate);
    Task<int> GetUserCountAsync();
    Task<int> GetUserCountByCompanyAsync(string companyId);
    Task<int> GetUserCountByRoleAsync(string role);
}
