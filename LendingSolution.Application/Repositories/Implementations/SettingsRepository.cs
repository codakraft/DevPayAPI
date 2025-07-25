using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class SettingsRepository : ISettingsRepository
{
    private readonly ApplicationDbContext _context;

    public SettingsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Settings?> GetSettingsAsync()
    {
        return await _context.Settings.FirstOrDefaultAsync();
    }

    public async Task<Settings> CreateSettingsAsync(Settings settings)
    {
        _context.Settings.Add(settings);
        await _context.SaveChangesAsync();
        return settings;
    }

    public async Task<bool> UpdateSettingsAsync(Settings settings)
    {
        try
        {
            _context.Settings.Update(settings);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
        catch
        {
            return false;
        }
    }
}
