using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Application log entry persisted to the database
/// </summary>
public class AppLog
{
    public long Id { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(20)]
    public string Level { get; set; } = string.Empty;  // Information, Warning, Error, Critical, Debug

    [MaxLength(512)]
    public string? Category { get; set; }  // Logger category (e.g. "LendingSolution.Application.Services.Implementations.RemitaService")

    public string Message { get; set; } = string.Empty;

    public string? Exception { get; set; }

    [MaxLength(100)]
    public string? EventId { get; set; }
}
