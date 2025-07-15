using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class PatchSystemSettingsDto
{
    [Range(0, double.MaxValue, ErrorMessage = "Legal fees must be a positive value")]
    public decimal? LegalFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Management fees must be a positive value")]
    public decimal? ManagementFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Processing fees must be a positive value")]
    public decimal? ProcessingFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Penalty fees must be a positive value")]
    public decimal? PenaltyFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Late fees must be a positive value")]
    public decimal? LateFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Documentation fees must be a positive value")]
    public decimal? DocumentationFees { get; set; }
    
    [RegularExpression("^(PERCENTAGE|FIXED)$", ErrorMessage = "Legal fees type must be either PERCENTAGE or FIXED")]
    public string? LegalFeesType { get; set; }
    
    [RegularExpression("^(PERCENTAGE|FIXED)$", ErrorMessage = "Management fees type must be either PERCENTAGE or FIXED")]
    public string? ManagementFeesType { get; set; }
    
    [RegularExpression("^(PERCENTAGE|FIXED)$", ErrorMessage = "Processing fees type must be either PERCENTAGE or FIXED")]
    public string? ProcessingFeesType { get; set; }
}
