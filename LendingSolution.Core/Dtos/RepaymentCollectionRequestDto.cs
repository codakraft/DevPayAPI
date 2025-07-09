using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class RepaymentCollectionRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string MandateId { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string PayerAccount { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string PayerBankCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Description { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Reference { get; set; }

    public DateTime? CollectionDate { get; set; }

    [Required]
    [StringLength(20)]
    public string CollectionType { get; set; } = "STANDING_ORDER";
}
