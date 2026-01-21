using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class Settings : ExtendedBase
{
    [Range(0, double.MaxValue, ErrorMessage = "Legal fee must be a positive value")]
    public decimal LegalFee { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Maintenance fee must be a positive value")]
    public decimal MaintenanceFee { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Processing fee must be a positive value")]
    public decimal ProcessingFee { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Penalty fee must be a positive value")]
    public decimal PenaltyFee { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "Late fee must be a positive value")]
    public decimal LateFee { get; set; } = 0;
    
    [Range(0, double.MaxValue, ErrorMessage = "OTP fee must be a positive value")]
    public decimal OtpFee { get; set; } = 20;
    
    [Range(0, double.MaxValue, ErrorMessage = "Documentation fee must be a positive value")]
    public decimal DocumentationFee { get; set; } = 0;
    
    public FeeType OtpFeeType { get; set; } = FeeType.FIXED;
    public FeeType LegalFeeType { get; set; } = FeeType.FIXED;
    public FeeType MaintenanceFeeType { get; set; } = FeeType.FIXED;
    public FeeType ProcessingFeeType { get; set; } = FeeType.FIXED;
}

public enum FeeType
{
    FIXED = 1,
    PERCENTAGE = 2
}
