using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class AdminSettingsService(IAdminSettingsRepository adminSettingsRepository) : IAdminSettingsService
{
    private readonly IAdminSettingsRepository _adminSettingsRepository = adminSettingsRepository;

    public async Task<List<AdminSettingsDto>> GetAllSettingsAsync()
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

        return settingDtos;
    }

    public async Task<AdminSettingsDto> GetSettingByKeyAsync(string settingKey)
    {
        var setting = await _adminSettingsRepository.GetSettingByKeyAsync(settingKey) ?? throw new AppException("Setting not found", 404);
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

        return settingDto;
    }

    public async Task<AdminSettingsDto> GetSettingByIdAsync(string id)
    {
        var setting = await _adminSettingsRepository.GetSettingByIdAsync(id) ?? throw new AppException("Setting not found", 404);
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

        return settingDto;
    }

    public async Task<AdminSettingsDto> CreateSettingAsync(UpdateAdminSettingsDto settingDto, string userId)
    {
        // Check if setting with same key already exists
        var existingSetting = await _adminSettingsRepository.GetSettingByKeyAsync(settingDto.SettingKey);
        if (existingSetting != null)
        {
            throw new AppException("Setting with this key already exists", 409);
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

        var result = await _adminSettingsRepository.CreateSettingAsync(setting);
        
        // Check if the repository returns an ApiResponse (legacy)
        if (result is ApiResponse apiResponse)
        {
            if (!apiResponse.Success)
            {
                throw new AppException(apiResponse.Message);
            }
            
            // If the repository returns ApiResponse, we need to get the created setting
            var createdSetting = await _adminSettingsRepository.GetSettingByKeyAsync(settingDto.SettingKey);
            if (createdSetting == null)
            {
                throw new AppException("Failed to retrieve created setting");
            }
            
            return new AdminSettingsDto
            {
                Id = createdSetting.Id,
                SettingKey = createdSetting.SettingKey,
                SettingValue = createdSetting.SettingValue,
                Description = createdSetting.Description ?? string.Empty,
                IsActive = createdSetting.IsActive,
                CreatedAt = createdSetting.CreatedAt,
                UpdatedAt = createdSetting.UpdatedAt
            };
        }
        
        // If repository returns the actual setting object
        return new AdminSettingsDto
        {
            Id = setting.Id,
            SettingKey = setting.SettingKey,
            SettingValue = setting.SettingValue,
            Description = setting.Description ?? string.Empty,
            IsActive = setting.IsActive,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt
        };
    }

    public async Task<AdminSettingsDto> UpdateSettingAsync(string id, UpdateAdminSettingsDto settingDto, string userId)
    {
        var existingSetting = await _adminSettingsRepository.GetSettingByIdAsync(id);
        if (existingSetting == null)
        {
            throw new AppException("Setting not found", 404);
        }

        // Check if another setting with same key exists (excluding current one)
        var duplicateSetting = await _adminSettingsRepository.GetSettingByKeyAsync(settingDto.SettingKey);
        if (duplicateSetting != null && duplicateSetting.Id != id)
        {
            throw new AppException("Another setting with this key already exists", 409);
        }

        existingSetting.SettingKey = settingDto.SettingKey;
        existingSetting.SettingValue = settingDto.SettingValue;
        existingSetting.Description = settingDto.Description;
        existingSetting.IsActive = settingDto.IsActive;
        existingSetting.UpdatedBy = userId;

        var result = await _adminSettingsRepository.UpdateSettingAsync(existingSetting);
        
        // Check if the repository returns an ApiResponse (legacy)
        if (result is ApiResponse apiResponse)
        {
            if (!apiResponse.Success)
            {
                throw new AppException(apiResponse.Message);
            }
        }

        // Return the updated setting DTO
        return new AdminSettingsDto
        {
            Id = existingSetting.Id,
            SettingKey = existingSetting.SettingKey,
            SettingValue = existingSetting.SettingValue,
            Description = existingSetting.Description ?? string.Empty,
            IsActive = existingSetting.IsActive,
            CreatedAt = existingSetting.CreatedAt,
            UpdatedAt = existingSetting.UpdatedAt
        };
    }

    public async Task DeleteSettingAsync(string id)
    {
        var result = await _adminSettingsRepository.DeleteSettingAsync(id);
        
        // Check if the repository returns an ApiResponse (legacy)
        if (result is ApiResponse apiResponse)
        {
            if (!apiResponse.Success)
            {
                throw new AppException(apiResponse.Message);
            }
        }
    }
}
