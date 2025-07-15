using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class SystemSettingsRepository : ISystemSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SystemSettings?> GetActiveSettingsAsync()
    {
        return await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.IsActive);
    }

    public async Task<SystemSettings?> GetSettingsByIdAsync(Guid id)
    {
        return await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<SystemSettings> CreateSettingsAsync(SystemSettings settings)
    {
        _context.SystemSettings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<bool> UpdateSettingsAsync(SystemSettings settings)
    {
        try
        {
            _context.SystemSettings.Update(settings);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateAllSettingsAsync()
    {
        try
        {
            var activeSettings = await _context.SystemSettings
                .Where(s => s.IsActive)
                .ToListAsync();
                
            foreach (var setting in activeSettings)
            {
                setting.IsActive = false;
                setting.UpdatedAt = DateTime.UtcNow;
            }
            
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<SystemSettings>> GetAllSettingsAsync()
    {
        return await _context.SystemSettings
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }
}
