namespace LendingSolution.Core.Dtos.Response.Remita;

public class BankDto
{
    public string BankCode { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Type { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BanksResponseDto : ResponseBase
{
    public List<BankDto> Banks { get; set; } = new();
    public int TotalCount { get; set; }
    public DateTime LastUpdated { get; set; }
}
