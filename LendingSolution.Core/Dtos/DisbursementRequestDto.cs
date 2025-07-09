using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class RemitaDisbursementRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(10)]
    public string BeneficiaryAccount { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string BeneficiaryBankCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string BeneficiaryName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Narration { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Reference { get; set; }

    [Required]
    [StringLength(10)]
    public string DebitAccount { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string DebitBankCode { get; set; } = string.Empty;

    public DateTime? ValueDate { get; set; }
}
