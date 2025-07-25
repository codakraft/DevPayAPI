using System.ComponentModel.DataAnnotations;
using LendingSolution.Core.Models;

namespace LendingSolution.Core.Dtos;

public class SettingsDto
{
    public Guid Id { get; set; }
    public decimal LegalFee { get; set; }
    public decimal ManagementFee { get; set; }
    public decimal ProcessingFee { get; set; }
    public decimal PenaltyFee { get; set; }
    public decimal LateFee { get; set; }
    public decimal OtpFee { get; set; }
    public decimal DocumentationFee { get; set; }
    public FeeType OtpFeeType { get; set; }
    public string OtpFeeTypeName => OtpFeeType.ToString();
    public FeeType LegalFeeType { get; set; }
    public string LegalFeeTypeName => LegalFeeType.ToString();
    public FeeType ManagementFeeType { get; set; }
    public string ManagementFeeTypeName => ManagementFeeType.ToString();
    public FeeType ProcessingFeeType { get; set; }
    public string ProcessingFeeTypeName => ProcessingFeeType.ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class UpdateSettingsDto
{
    [Range(0, double.MaxValue, ErrorMessage = "Legal fee must be a positive value")]
    public decimal? LegalFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Management fee must be a positive value")]
    public decimal? ManagementFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Processing fee must be a positive value")]
    public decimal? ProcessingFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Penalty fee must be a positive value")]
    public decimal? PenaltyFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Late fee must be a positive value")]
    public decimal? LateFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "OTP fee must be a positive value")]
    public decimal? OtpFee { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Documentation fee must be a positive value")]
    public decimal? DocumentationFee { get; set; }
    
    public FeeType? OtpFeeType { get; set; }
    public FeeType? LegalFeeType { get; set; }
    public FeeType? ManagementFeeType { get; set; }
    public FeeType? ProcessingFeeType { get; set; }
}
