namespace LendingSolution.Core.Dtos;

public class LoginResponseDto
{
    public string Token { get; set; }
    public string UserId { get; set; }
    public string CompanyId { get; set; }
    public string Role { get; set; }
    public List<UserCompanyDto> Companies { get; set; } // If multiple companies, return this for selection
}
