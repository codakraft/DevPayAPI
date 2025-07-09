using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class AccountVerificationRequestDto
{
    [Required]
    [StringLength(10)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string AccountName { get; set; } = string.Empty;

    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string Bvn { get; set; } = string.Empty;
}
