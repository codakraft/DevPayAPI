using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

public interface IEmbedlyWalletService
{
    Task<EmbedlyCreateCustomerResponseDto?> CreateCustomerAsync(EmbedlyCreateCustomerRequestDto request);
    Task<EmbedlyCreateCustomerV2ResponseDto?> CreateCustomerV2Async(EmbedlyCreateCustomerV2RequestDto request);
    Task<EmbedlyUpdateCustomerV2ResponseDto?> UpdateCustomerV2Async(string customerId, EmbedlyUpdateCustomerV2RequestDto request);
    Task<EmbedlyGetCustomerResponseDto?> GetCustomerByIdAsync(string customerId);
    Task<EmbedlyGetAllCustomersResponseDto?> GetAllCustomersAsync();
    Task<EmbedlyCreateWalletResponseDto?> CreateWalletAsync(EmbedlyCreateWalletRequestDto request);
    Task<EmbedlyGetWalletResponseDto?> GetWalletByIdAsync(string walletId);
    Task<EmbedlyGetWalletResponseDto?> GetWalletByAccountNumberAsync(string accountNumber);
    Task<EmbedlyGetWalletsByCustomerResponseDto?> GetWalletsByCustomerIdAsync(string customerId);
    Task<EmbedlyGetKycStatusResponseDto?> GetKycStatusAsync(string customerId);
    Task<EmbedlyFundAccountResponseDto?> FundAccountAsync(EmbedlyFundAccountRequestDto request);
    Task<EmbedlyWalletTransferResponseDto?> WalletTransferAsync(EmbedlyWalletTransferRequestDto request);
    Task<EmbedlyKycUpgradeResponseDto?> NinKycUpgradeAsync(string customerId, string nin, EmbedlyNinKycUpgradeRequestDto request);
    Task<EmbedlyKycUpgradeResponseDto?> BvnKycUpgradeAsync(EmbedlyBvnKycUpgradeRequestDto request);
}
