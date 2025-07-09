namespace LendingSolution.Core.Dtos.Response.Remita;

public class AccountVerificationResponseDto : ResponseBase
{
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public string Bvn { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Gender { get; set; }
}
