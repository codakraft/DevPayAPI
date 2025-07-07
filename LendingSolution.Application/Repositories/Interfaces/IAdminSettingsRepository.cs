using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IAdminSettingsRepository
{
    Task<IEnumerable<AdminSettings>> GetAllSettingsAsync();
    Task<AdminSettings?> GetSettingByKeyAsync(string settingKey);
    Task<AdminSettings?> GetSettingByIdAsync(string id);
    Task<ApiResponse> CreateSettingAsync(AdminSettings setting);
    Task<ApiResponse> UpdateSettingAsync(AdminSettings setting);
    Task<ApiResponse> DeleteSettingAsync(string id);
    Task<bool> SettingExistsAsync(string settingKey);
}
