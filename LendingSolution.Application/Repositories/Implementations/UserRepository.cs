using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Application.Repositories.Interfaces;

namespace LendingSolution.Application.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<List<ApplicationUser>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string role)
    {
        return (await _userManager.GetUsersInRoleAsync(role)).ToList();
    }

    public async Task<List<ApplicationUser>> GetUsersByCompanyIdAsync(string companyId)
    {
        return await _userManager.Users
            .Where(u => u.CompanyId == companyId)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetActiveUsersAsync()
    {
        return await _userManager.Users
            .Where(u => u.IsActive)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetUsersByGenderAsync(string gender)
    {
        return await _userManager.Users
            .Where(u => u.Gender == gender)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetUsersCreatedAfterAsync(DateTime date)
    {
        return await _userManager.Users
            .Where(u => u.CreatedAt >= date)
            .ToListAsync();
    }

    public async Task<List<ApplicationUser>> GetUsersCreatedBetweenAsync(DateTime startDate, DateTime endDate)
    {
        return await _userManager.Users
            .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
            .ToListAsync();
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _userManager.Users.CountAsync();
    }

    public async Task<int> GetUserCountByCompanyAsync(string companyId)
    {
        return await _userManager.Users
            .Where(u => u.CompanyId == companyId)
            .CountAsync();
    }

    public async Task<int> GetUserCountByRoleAsync(string role)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        return usersInRole.Count;
    }
}
