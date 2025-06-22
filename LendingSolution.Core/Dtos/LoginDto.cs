using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class LoginRequestDto
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class LoginResponseDto
{
    public string? Token { get; set; } = null;
    public string? UserId { get; set; } = null;
    public string? Role { get; set; }
}
