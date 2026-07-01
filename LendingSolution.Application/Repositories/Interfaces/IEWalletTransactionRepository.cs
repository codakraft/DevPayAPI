using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IEWalletTransactionRepository
{
    Task<EWalletTransaction> CreateAsync(EWalletTransaction transaction);
    Task<EWalletTransaction?> GetByReferenceAsync(string transactionReference);
    Task<List<EWalletTransaction>> GetByAccountNumberAsync(string accountNumber, int page = 1, int pageSize = 20);
}
