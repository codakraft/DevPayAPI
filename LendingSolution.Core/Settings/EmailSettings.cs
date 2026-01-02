namespace LendingSolution.Core.Settings;

/// <summary>
/// Email configuration settings for SMTP
/// </summary>
public class EmailSettings
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "LendingSolution";
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>
    /// When true, allows email simulation (logging only) if SMTP sending fails.
    /// Useful for development/demo environments.
    /// </summary>
    public bool AllowEmailSimulationOnFailure { get; set; } = false;
}
