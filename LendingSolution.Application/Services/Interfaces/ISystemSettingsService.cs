using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface ISystemSettingsService
{
    Task<SystemSettingsDto> GetActiveSettingsAsync();
    Task<SystemSettingsDto> GetSettingsByIdAsync(Guid id);
    Task<SystemSettingsDto> CreateSettingsAsync(UpdateSystemSettingsDto settingsDto, string userId);
    Task<SystemSettingsDto> UpdateSettingsAsync(Guid id, UpdateSystemSettingsDto settingsDto, string userId);
    Task<List<SystemSettingsDto>> GetAllSettingsAsync();
}
