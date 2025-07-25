using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface ISettingsRepository
{
    Task<Settings?> GetSettingsAsync();
    Task<Settings> CreateSettingsAsync(Settings settings);
    Task<bool> UpdateSettingsAsync(Settings settings);
}
