using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ISystemSettingsRepository
{
    Task<SystemSettings?> GetActiveSettingsAsync();
    Task<SystemSettings?> GetSettingsByIdAsync(Guid id);
    Task<SystemSettings> CreateSettingsAsync(SystemSettings settings);
    Task<bool> UpdateSettingsAsync(SystemSettings settings);
    Task<bool> DeactivateAllSettingsAsync();
    Task<List<SystemSettings>> GetAllSettingsAsync();
}
