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

    [Required]
    [StringLength(50)]
    public string MiddleName { get; set; } = string.Empty;

    [Required]
    [StringLength(10)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(3)]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(1)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string State { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LocalGovernmentArea { get; set; } = string.Empty;

    public int? MonthsOfHistory { get; set; } = 6;
}
