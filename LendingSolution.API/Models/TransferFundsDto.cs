namespace LendingSolution.API.Models;

/// <summary>
/// DTO for fund transfer between wallets
/// </summary>
public class TransferFundsDto
{
    public Guid FromWalletId { get; set; }
    public Guid ToWalletId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
}
