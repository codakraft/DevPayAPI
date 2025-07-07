using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Infrastructure.Data;

namespace LendingSolution.Application.Repositories.Implementations;

public class AdminSettingsRepository(ApplicationDbContext context) : IAdminSettingsRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<AdminSettings>> GetAllSettingsAsync()
    {
        return await _context.AdminSettings
            .OrderBy(s => s.SettingKey)
            .ToListAsync();
    }

    public async Task<AdminSettings?> GetSettingByKeyAsync(string settingKey)
    {
        return await _context.AdminSettings
            .FirstOrDefaultAsync(s => s.SettingKey == settingKey);
    }

    public async Task<AdminSettings?> GetSettingByIdAsync(string id)
    {
        return await _context.AdminSettings
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ApiResponse> CreateSettingAsync(AdminSettings setting)
    {
        try
        {
            setting.CreatedAt = DateTime.UtcNow;
            setting.UpdatedAt = DateTime.UtcNow;
            
            await _context.AdminSettings.AddAsync(setting);
            await _context.SaveChangesAsync();
            
            return ApiResponse.Ok("Setting created successfully", setting);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to create setting: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateSettingAsync(AdminSettings setting)
    {
        try
        {
            setting.UpdatedAt = DateTime.UtcNow;
            
            _context.AdminSettings.Update(setting);
            await _context.SaveChangesAsync();
            
            return ApiResponse.Ok("Setting updated successfully", setting);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to update setting: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteSettingAsync(string id)
    {
        try
        {
            var setting = await GetSettingByIdAsync(id);
            if (setting == null)
            {
                return ApiResponse.Fail("Setting not found");
            }

            _context.AdminSettings.Remove(setting);
            await _context.SaveChangesAsync();
            
            return ApiResponse.Ok("Setting deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to delete setting: {ex.Message}");
        }
    }

    public async Task<bool> SettingExistsAsync(string settingKey)
    {
        return await _context.AdminSettings
            .AnyAsync(s => s.SettingKey == settingKey);
    }
}
