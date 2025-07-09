using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAdminSettingsService
{
    Task<List<AdminSettingsDto>> GetAllSettingsAsync();
    Task<AdminSettingsDto> GetSettingByKeyAsync(string settingKey);
    Task<AdminSettingsDto> GetSettingByIdAsync(string id);
    Task<AdminSettingsDto> CreateSettingAsync(UpdateAdminSettingsDto settingDto, string userId);
    Task<AdminSettingsDto> UpdateSettingAsync(string id, UpdateAdminSettingsDto settingDto, string userId);
    Task DeleteSettingAsync(string id);
}
