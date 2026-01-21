using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Implementations;

public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repository;

    public SettingsService(ISettingsRepository repository)
    {
        _repository = repository;
    }

    public async Task<SettingsDto> GetSettingsAsync()
    {
        var settings = await _repository.GetSettingsAsync();
        
        // If no settings exist, create default settings
        if (settings == null)
        {
            settings = new Settings
            {
                LegalFee = 0,
                MaintenanceFee = 0,
                ProcessingFee = 0,
                PenaltyFee = 0,
                LateFee = 0,
                OtpFee = 20,
                DocumentationFee = 0,
                OtpFeeType = FeeType.FIXED,
                LegalFeeType = FeeType.FIXED,
                MaintenanceFeeType = FeeType.FIXED,
                ProcessingFeeType = FeeType.FIXED,
                CreatedBy = "System"
            };
            
            settings = await _repository.CreateSettingsAsync(settings);
        }

        return MapToDto(settings);
    }

    public async Task<SettingsDto> UpdateSettingsAsync(UpdateSettingsDto settingsDto, string userId)
    {
        var existingSettings = await _repository.GetSettingsAsync();
        
        if (existingSettings == null)
        {
            throw new AppException("Settings not found. Please get settings first to initialize them.", 404);
        }

        // Update only provided values
        if (settingsDto.LegalFee.HasValue)
            existingSettings.LegalFee = settingsDto.LegalFee.Value;
            
        if (settingsDto.MaintenanceFee.HasValue)
            existingSettings.MaintenanceFee = settingsDto.MaintenanceFee.Value;
            
        if (settingsDto.ProcessingFee.HasValue)
            existingSettings.ProcessingFee = settingsDto.ProcessingFee.Value;
            
        if (settingsDto.PenaltyFee.HasValue)
            existingSettings.PenaltyFee = settingsDto.PenaltyFee.Value;
            
        if (settingsDto.LateFee.HasValue)
            existingSettings.LateFee = settingsDto.LateFee.Value;
            
        if (settingsDto.OtpFee.HasValue)
            existingSettings.OtpFee = settingsDto.OtpFee.Value;
            
        if (settingsDto.DocumentationFee.HasValue)
            existingSettings.DocumentationFee = settingsDto.DocumentationFee.Value;
            
        if (settingsDto.OtpFeeType.HasValue)
            existingSettings.OtpFeeType = settingsDto.OtpFeeType.Value;
            
        if (settingsDto.LegalFeeType.HasValue)
            existingSettings.LegalFeeType = settingsDto.LegalFeeType.Value;
            
        if (settingsDto.MaintenanceFeeType.HasValue)
            existingSettings.MaintenanceFeeType = settingsDto.MaintenanceFeeType.Value;
            
        if (settingsDto.ProcessingFeeType.HasValue)
            existingSettings.ProcessingFeeType = settingsDto.ProcessingFeeType.Value;

        existingSettings.UpdatedBy = userId;
        existingSettings.UpdatedAt = DateTime.UtcNow;

        var success = await _repository.UpdateSettingsAsync(existingSettings);
        if (!success)
            throw new AppException("Failed to update settings", 500);

        return MapToDto(existingSettings);
    }

    private static SettingsDto MapToDto(Settings settings)
    {
        return new SettingsDto
        {
            Id = settings.Id,
            LegalFee = settings.LegalFee,
            MaintenanceFee = settings.MaintenanceFee,
            ProcessingFee = settings.ProcessingFee,
            PenaltyFee = settings.PenaltyFee,
            LateFee = settings.LateFee,
            OtpFee = settings.OtpFee,
            DocumentationFee = settings.DocumentationFee,
            OtpFeeType = settings.OtpFeeType,
            LegalFeeType = settings.LegalFeeType,
            MaintenanceFeeType = settings.MaintenanceFeeType,
            ProcessingFeeType = settings.ProcessingFeeType,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt,
            CreatedBy = settings.CreatedBy,
            UpdatedBy = settings.UpdatedBy
        };
    }
}
