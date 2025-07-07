namespace LendingSolution.Core.Dtos;

public class AdminSettingsDto
{
    public string Id { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateAdminSettingsDto
{
    public required string SettingKey { get; set; }
    public required string SettingValue { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
