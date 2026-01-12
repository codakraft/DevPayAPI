using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO for testing Providus disbursement functionality
/// </summary>
public class TestDisbursementRequestDto
{
    /// <summary>
    /// The destination account number
    /// </summary>
    [Required(ErrorMessage = "Account number is required")]
    public string AccountNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// The destination bank code
    /// </summary>
    [Required(ErrorMessage = "Bank code is required")]
    public string BankCode { get; set; } = string.Empty;
    
    /// <summary>
    /// The amount to transfer
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Narration for the transaction
    /// </summary>
    public string? Narration { get; set; }
    
    /// <summary>
    /// Beneficiary name for reference
    /// </summary>
    public string? BeneficiaryName { get; set; }
}
