using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface ISettingsService
{
    Task<SettingsDto> GetSettingsAsync();
    Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsDto settingsDto, string userId);
}
