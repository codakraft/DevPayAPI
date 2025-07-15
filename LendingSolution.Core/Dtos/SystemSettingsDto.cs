using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class SystemSettingsDto
{
    public Guid Id { get; set; }
    public decimal LegalFees { get; set; }
    public decimal ManagementFees { get; set; }
    public decimal ProcessingFees { get; set; }
    public decimal? PenaltyFees { get; set; }
    public decimal? LateFees { get; set; }
    public decimal? DocumentationFees { get; set; }
    public string LegalFeesType { get; set; } = string.Empty;
    public string ManagementFeesType { get; set; } = string.Empty;
    public string ProcessingFeesType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateSystemSettingsDto
{
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Legal fees must be a positive value")]
    public decimal LegalFees { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Management fees must be a positive value")]
    public decimal ManagementFees { get; set; }
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Processing fees must be a positive value")]
    public decimal ProcessingFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Penalty fees must be a positive value")]
    public decimal? PenaltyFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Late fees must be a positive value")]
    public decimal? LateFees { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Documentation fees must be a positive value")]
    public decimal? DocumentationFees { get; set; }
    
    [Required]
    [RegularExpression("^(PERCENTAGE|FIXED)$", ErrorMessage = "Legal fees type must be either PERCENTAGE or FIXED")]
    public string LegalFeesType { get; set; } = "PERCENTAGE";
    
    [Required]
    [RegularExpression("^(PERCENTAGE|FIXED)$", ErrorMessage = "Management fees type must be either PERCENTAGE or FIXED")]
    public string ManagementFeesType { get; set; } = "PERCENTAGE";
    
    [Required]
    [RegularExpression("^(PERCENTAGE|FIXED)$", ErrorMessage = "Processing fees type must be either PERCENTAGE or FIXED")]
    public string ProcessingFeesType { get; set; } = "PERCENTAGE";
}
