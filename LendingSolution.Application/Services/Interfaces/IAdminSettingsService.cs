using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface IAdminSettingsService
{
    Task<ApiResponse> GetAllSettingsAsync();
    Task<ApiResponse> GetSettingByKeyAsync(string settingKey);
    Task<ApiResponse> GetSettingByIdAsync(string id);
    Task<ApiResponse> CreateSettingAsync(UpdateAdminSettingsDto settingDto, string userId);
    Task<ApiResponse> UpdateSettingAsync(string id, UpdateAdminSettingsDto settingDto, string userId);
    Task<ApiResponse> DeleteSettingAsync(string id);
}
