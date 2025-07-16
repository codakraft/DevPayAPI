using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Repositories.Interfaces;

/// <summary>
/// Repository interface for wallet operations
/// </summary>
public interface IWalletRepository
{
    /// <summary>
    /// Get wallet by ID
    /// </summary>
    Task<Wallet?> GetWalletByIdAsync(Guid walletId);
    
    /// <summary>
    /// Get wallet by company ID
    /// </summary>
    Task<Wallet?> GetWalletByCompanyIdAsync(Guid companyId);
    
    /// <summary>
    /// Get SuperAdmin wallet
    /// </summary>
    Task<Wallet?> GetSuperAdminWalletAsync();
    
    /// <summary>
    /// Create a new wallet
    /// </summary>
    Task<Wallet> CreateWalletAsync(Wallet wallet);
    
    /// <summary>
    /// Update wallet
    /// </summary>
    Task<Wallet> UpdateWalletAsync(Wallet wallet);
    
    /// <summary>
    /// Get all wallets
    /// </summary>
    Task<List<Wallet>> GetAllWalletsAsync();
    
    /// <summary>
    /// Check if wallet has sufficient balance
    /// </summary>
    Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount);
}

/// <summary>
/// Repository interface for wallet transaction operations
/// </summary>
public interface IWalletTransactionRepository
{
    /// <summary>
    /// Create a new wallet transaction
    /// </summary>
    Task<WalletTransaction> CreateTransactionAsync(WalletTransaction transaction);
    
    /// <summary>
    /// Get transactions by wallet ID
    /// </summary>
    Task<List<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Get transaction by ID
    /// </summary>
    Task<WalletTransaction?> GetTransactionByIdAsync(Guid transactionId);
    
    /// <summary>
    /// Get transaction by Paystack reference
    /// </summary>
    Task<WalletTransaction?> GetTransactionByPaystackReferenceAsync(string paystackReference);
    
    /// <summary>
    /// Get transactions by query parameters
    /// </summary>
    Task<List<WalletTransaction>> GetTransactionsByQueryAsync(WalletTransactionQueryDto query);
    
    /// <summary>
    /// Get total transaction count for wallet
    /// </summary>
    Task<int> GetTransactionCountAsync(Guid walletId);
}
