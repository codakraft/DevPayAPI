namespace LendingSolution.Core.Dtos;

public class CreateSuperAdminDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateSuperAdminRequestDto : CreateSuperAdminDto;