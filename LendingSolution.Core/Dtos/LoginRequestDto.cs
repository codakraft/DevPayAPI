using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class LoginRequestDto
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
