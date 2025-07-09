using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class CreateMandateRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    [Range(1, 60)]
    public int TenorInMonths { get; set; }

    [Required]
    [StringLength(10)]
    public string Frequency { get; set; } = "Monthly";

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(50)]
    public string PayerName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string PayerEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PayerPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string PayerBankCode { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string PayerAccount { get; set; } = string.Empty;

    [Required]
    [StringLength(2)]
    public string MandateType { get; set; } = "SO"; // Standing Order

    [StringLength(200)]
    public string? Description { get; set; }
}
