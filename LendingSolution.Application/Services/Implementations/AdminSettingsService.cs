using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Repositories.Interfaces;

namespace LendingSolution.Application.Services.Implementations;

public class AdminSettingsService(IAdminSettingsRepository adminSettingsRepository) : IAdminSettingsService
{
    private readonly IAdminSettingsRepository _adminSettingsRepository = adminSettingsRepository;

    public async Task<ApiResponse> GetAllSettingsAsync()
    {
        try
        {
            var settings = await _adminSettingsRepository.GetAllSettingsAsync();
            
            var settingDtos = settings.Select(s => new AdminSettingsDto
            {
                Id = s.Id,
                SettingKey = s.SettingKey,
                SettingValue = s.SettingValue,
                Description = s.Description ?? string.Empty,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList();

            return ApiResponse.Ok("Settings retrieved successfully", settingDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve settings: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetSettingByKeyAsync(string settingKey)
    {
        try
        {
            var setting = await _adminSettingsRepository.GetSettingByKeyAsync(settingKey);
            
            if (setting == null)
            {
                return ApiResponse.Fail("Setting not found");
            }

            var settingDto = new AdminSettingsDto
            {
                Id = setting.Id,
                SettingKey = setting.SettingKey,
                SettingValue = setting.SettingValue,
                Description = setting.Description ?? string.Empty,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return ApiResponse.Ok("Setting retrieved successfully", settingDto);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve setting: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetSettingByIdAsync(string id)
    {
        try
        {
            var setting = await _adminSettingsRepository.GetSettingByIdAsync(id);
            
            if (setting == null)
            {
                return ApiResponse.Fail("Setting not found");
            }

            var settingDto = new AdminSettingsDto
            {
                Id = setting.Id,
                SettingKey = setting.SettingKey,
                SettingValue = setting.SettingValue,
                Description = setting.Description ?? string.Empty,
                IsActive = setting.IsActive,
                CreatedAt = setting.CreatedAt,
                UpdatedAt = setting.UpdatedAt
            };

            return ApiResponse.Ok("Setting retrieved successfully", settingDto);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve setting: {ex.Message}");
        }
    }

    public async Task<ApiResponse> CreateSettingAsync(UpdateAdminSettingsDto settingDto, string userId)
    {
        try
        {
            // Check if setting with same key already exists
            var existingSetting = await _adminSettingsRepository.GetSettingByKeyAsync(settingDto.SettingKey);
            if (existingSetting != null)
            {
                return ApiResponse.Fail("Setting with this key already exists");
            }

            var setting = new AdminSettings
            {
                SettingKey = settingDto.SettingKey,
                SettingValue = settingDto.SettingValue,
                Description = settingDto.Description,
                IsActive = settingDto.IsActive,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            return await _adminSettingsRepository.CreateSettingAsync(setting);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to create setting: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateSettingAsync(string id, UpdateAdminSettingsDto settingDto, string userId)
    {
        try
        {
            var existingSetting = await _adminSettingsRepository.GetSettingByIdAsync(id);
            if (existingSetting == null)
            {
                return ApiResponse.Fail("Setting not found");
            }

            // Check if another setting with same key exists (excluding current one)
            var duplicateSetting = await _adminSettingsRepository.GetSettingByKeyAsync(settingDto.SettingKey);
            if (duplicateSetting != null && duplicateSetting.Id != id)
            {
                return ApiResponse.Fail("Another setting with this key already exists");
            }

            existingSetting.SettingKey = settingDto.SettingKey;
            existingSetting.SettingValue = settingDto.SettingValue;
            existingSetting.Description = settingDto.Description;
            existingSetting.IsActive = settingDto.IsActive;
            existingSetting.UpdatedBy = userId;

            return await _adminSettingsRepository.UpdateSettingAsync(existingSetting);
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
            return await _adminSettingsRepository.DeleteSettingAsync(id);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to delete setting: {ex.Message}");
        }
    }
}
