using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Bvn { get; set; }

    [Required]
    public required DateTime DateOfBirth { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public Guid ProductId { get; set; }
}