using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class SystemSettings : ExtendedBase
{
    [Required]
    public decimal LegalFees { get; set; } = 0;

    [Required]
    public decimal ManagementFees { get; set; } = 0;

    [Required]
    public decimal ProcessingFees { get; set; } = 0;

    // Additional fee types that might be needed
    public decimal? PenaltyFees { get; set; }
    public decimal? LateFees { get; set; }
    public decimal? DocumentationFees { get; set; }
    public decimal? OtpCharges { get; set; }

    // Fee calculation types
    public string LegalFeesType { get; set; } = "PERCENTAGE"; // PERCENTAGE or FIXED
    public string ManagementFeesType { get; set; } = "PERCENTAGE"; // PERCENTAGE or FIXED
    public string ProcessingFeesType { get; set; } = "PERCENTAGE"; // PERCENTAGE or FIXED
    public string OtpChargesType { get; set; } = "FIXED"; // PERCENTAGE or FIXED

    public bool IsActive { get; set; } = true;
}
