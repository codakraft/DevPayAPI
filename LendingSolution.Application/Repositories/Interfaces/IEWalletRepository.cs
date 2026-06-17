using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IEWalletRepository
{
    Task<EWallet> CreateAsync(EWallet eWallet);
    Task<EWallet> UpdateAsync(EWallet eWallet);
    Task<EWallet?> GetByMobileNumberAsync(string mobileNumber);
    Task<EWallet?> GetByEmbedlyCustomerIdAsync(string embedlyCustomerId);
    Task<EWallet?> GetByEmbedlyWalletIdAsync(string embedlyWalletId);
    Task<EWallet?> GetByAccountNumberAsync(string accountNumber);
}
