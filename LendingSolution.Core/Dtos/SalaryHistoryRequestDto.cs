using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class SalaryHistoryRequestDto
{
    [Required]
    [StringLength(11, MinimumLength = 11)]
    public string Bvn { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(10)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string BankCode { get; set; } = string.Empty;

    public string? AuthorisationCode { get; set; } = "";

    public string? AuthorisationChannel { get; set; } = "USSD";
}
