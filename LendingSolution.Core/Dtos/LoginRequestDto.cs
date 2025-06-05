using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class LoginRequestDto
{
    [Required]
    public required string EmailOrPhone { get; set; }

    [Required]
    public required string Password { get; set; }
}
