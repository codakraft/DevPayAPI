using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Implementations;

public class SystemSettingsService : ISystemSettingsService
{
    private readonly ISystemSettingsRepository _systemSettingsRepository;

    public SystemSettingsService(ISystemSettingsRepository systemSettingsRepository)
    {
        _systemSettingsRepository = systemSettingsRepository;
    }

    public async Task<SystemSettingsDto> GetActiveSettingsAsync()
    {
        var settings = await _systemSettingsRepository.GetActiveSettingsAsync();
        
        if (settings == null)
        {
            throw new AppException("No active system settings found", 404);
        }

        return MapToDto(settings);
    }

    public async Task<SystemSettingsDto> GetSettingsByIdAsync(Guid id)
    {
        var settings = await _systemSettingsRepository.GetSettingsByIdAsync(id);
        
        if (settings == null)
        {
            throw new AppException("System settings not found", 404);
        }

        return MapToDto(settings);
    }

    public async Task<SystemSettingsDto> CreateSettingsAsync(UpdateSystemSettingsDto settingsDto, string userId)
    {
        // Deactivate all existing settings first (we want only one active setting)
        await _systemSettingsRepository.DeactivateAllSettingsAsync();

        var settings = new SystemSettings
        {
            LegalFees = settingsDto.LegalFees,
            ManagementFees = settingsDto.ManagementFees,
            ProcessingFees = settingsDto.ProcessingFees,
            PenaltyFees = settingsDto.PenaltyFees,
            LateFees = settingsDto.LateFees,
            DocumentationFees = settingsDto.DocumentationFees,
            OtpCharges = settingsDto.OtpCharges,
            LegalFeesType = settingsDto.LegalFeesType,
            ManagementFeesType = settingsDto.ManagementFeesType,
            ProcessingFeesType = settingsDto.ProcessingFeesType,
            OtpChargesType = settingsDto.OtpChargesType,
            IsActive = true,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdSettings = await _systemSettingsRepository.CreateSettingsAsync(settings);
        return MapToDto(createdSettings);
    }

    public async Task<SystemSettingsDto> UpdateSettingsAsync(Guid id, UpdateSystemSettingsDto settingsDto, string userId)
    {
        var existingSettings = await _systemSettingsRepository.GetSettingsByIdAsync(id);
        
        if (existingSettings == null)
        {
            throw new AppException("System settings not found", 404);
        }

        // Update properties
        existingSettings.LegalFees = settingsDto.LegalFees;
        existingSettings.ManagementFees = settingsDto.ManagementFees;
        existingSettings.ProcessingFees = settingsDto.ProcessingFees;
        existingSettings.PenaltyFees = settingsDto.PenaltyFees;
        existingSettings.LateFees = settingsDto.LateFees;
        existingSettings.DocumentationFees = settingsDto.DocumentationFees;
        existingSettings.OtpCharges = settingsDto.OtpCharges;
        existingSettings.LegalFeesType = settingsDto.LegalFeesType;
        existingSettings.ManagementFeesType = settingsDto.ManagementFeesType;
        existingSettings.ProcessingFeesType = settingsDto.ProcessingFeesType;
        existingSettings.OtpChargesType = settingsDto.OtpChargesType;
        existingSettings.UpdatedBy = userId;
        existingSettings.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _systemSettingsRepository.UpdateSettingsAsync(existingSettings);
        
        if (!updateResult)
        {
            throw new AppException("Failed to update system settings", 500);
        }

        return MapToDto(existingSettings);
    }

    public async Task<SystemSettingsDto> PatchActiveSettingsAsync(PatchSystemSettingsDto settingsDto, string userId)
    {
        var activeSettings = await _systemSettingsRepository.GetActiveSettingsAsync();
        
        if (activeSettings == null)
        {
            throw new AppException("No active system settings found to update", 404);
        }

        // Update only the properties that are provided (not null)
        if (settingsDto.LegalFees.HasValue)
            activeSettings.LegalFees = settingsDto.LegalFees.Value;
            
        if (settingsDto.ManagementFees.HasValue)
            activeSettings.ManagementFees = settingsDto.ManagementFees.Value;
            
        if (settingsDto.ProcessingFees.HasValue)
            activeSettings.ProcessingFees = settingsDto.ProcessingFees.Value;
            
        if (settingsDto.PenaltyFees.HasValue)
            activeSettings.PenaltyFees = settingsDto.PenaltyFees.Value;
            
        if (settingsDto.LateFees.HasValue)
            activeSettings.LateFees = settingsDto.LateFees.Value;
            
        if (settingsDto.DocumentationFees.HasValue)
            activeSettings.DocumentationFees = settingsDto.DocumentationFees.Value;
            
        if (settingsDto.OtpCharges.HasValue)
            activeSettings.OtpCharges = settingsDto.OtpCharges.Value;
            
        if (!string.IsNullOrEmpty(settingsDto.LegalFeesType))
            activeSettings.LegalFeesType = settingsDto.LegalFeesType;
            
        if (!string.IsNullOrEmpty(settingsDto.ManagementFeesType))
            activeSettings.ManagementFeesType = settingsDto.ManagementFeesType;
            
        if (!string.IsNullOrEmpty(settingsDto.ProcessingFeesType))
            activeSettings.ProcessingFeesType = settingsDto.ProcessingFeesType;
            
        if (!string.IsNullOrEmpty(settingsDto.OtpChargesType))
            activeSettings.OtpChargesType = settingsDto.OtpChargesType;

        // Update audit fields
        activeSettings.UpdatedBy = userId;
        activeSettings.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _systemSettingsRepository.UpdateSettingsAsync(activeSettings);
        
        if (!updateResult)
        {
            throw new AppException("Failed to update system settings", 500);
        }

        return MapToDto(activeSettings);
    }

    public async Task<List<SystemSettingsDto>> GetAllSettingsAsync()
    {
        var settingsList = await _systemSettingsRepository.GetAllSettingsAsync();
        return settingsList.Select(MapToDto).ToList();
    }

    private static SystemSettingsDto MapToDto(SystemSettings settings)
    {
        return new SystemSettingsDto
        {
            Id = settings.Id,
            LegalFees = settings.LegalFees,
            ManagementFees = settings.ManagementFees,
            ProcessingFees = settings.ProcessingFees,
            PenaltyFees = settings.PenaltyFees,
            LateFees = settings.LateFees,
            DocumentationFees = settings.DocumentationFees,
            OtpCharges = settings.OtpCharges,
            LegalFeesType = settings.LegalFeesType,
            ManagementFeesType = settings.ManagementFeesType,
            ProcessingFeesType = settings.ProcessingFeesType,
            OtpChargesType = settings.OtpChargesType,
            IsActive = settings.IsActive,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt,
            CreatedBy = settings.CreatedBy,
            UpdatedBy = settings.UpdatedBy
        };
    }
}
