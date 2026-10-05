using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Service interface for Paystack payment operations
/// </summary>
public interface IPaystackService
{
    /// <summary>
    /// Initialize payment with Paystack
    /// </summary>
    Task<PaystackInitializationDto> InitializePaymentAsync(string email, decimal amount, string reference, string? callbackUrl = null);
    
    /// <summary>
    /// Verify payment with Paystack
    /// </summary>
    Task<bool> VerifyPaymentAsync(string reference);

    /// <summary>
    /// Verify a transaction with Paystack and return its status, amount and currency
    /// </summary>
    Task<PaystackVerificationResult> VerifyTransactionAsync(string reference);
    
    /// <summary>
    /// Get payment details from Paystack
    /// </summary>
    Task<dynamic?> GetPaymentDetailsAsync(string reference);
}

/// <summary>
/// Service interface for wallet operations
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Get wallet by ID
    /// </summary>
    Task<WalletDto?> GetWalletByIdAsync(Guid walletId);
    
    /// <summary>
    /// Get wallet by company ID
    /// </summary>
    Task<WalletDto?> GetWalletByCompanyIdAsync(Guid companyId);
    
    /// <summary>
    /// Get SuperAdmin wallet
    /// </summary>
    Task<WalletDto?> GetSuperAdminWalletAsync();
    
    /// <summary>
    /// Create wallet for company
    /// </summary>
    Task<WalletDto> CreateCompanyWalletAsync(Guid companyId);
    
    /// <summary>
    /// Create SuperAdmin wallet
    /// </summary>
    Task<WalletDto> CreateSuperAdminWalletAsync();
    
    /// <summary>
    /// Get all wallets (SuperAdmin only)
    /// </summary>
    Task<List<WalletDto>> GetAllWalletsAsync();
    
    /// <summary>
    /// Fund wallet using Paystack
    /// </summary>
    Task<PaystackInitializationDto> InitiateWalletFundingAsync(FundWalletDto fundWalletDto, string? userId);
    
    /// <summary>
    /// Verify a Paystack funding and credit the wallet exactly once.
    /// <paramref name="callerCompanyId"/> limits completion to that company's wallet; null for SuperAdmin.
    /// </summary>
    Task<WalletFundingResultDto> CompleteWalletFundingAsync(string paystackReference, Guid? callerCompanyId, string? userId);
    
    /// <summary>
    /// Debit wallet for fees
    /// </summary>
    Task<bool> DebitWalletAsync(DebitWalletDto debitWalletDto, string? userId);
    
    /// <summary>
    /// Credit wallet
    /// </summary>
    Task<bool> CreditWalletAsync(Guid walletId, decimal amount, string description, string? referenceId, string? userId);
    
    /// <summary>
    /// Check if wallet has sufficient balance
    /// </summary>
    Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount);
    
    /// <summary>
    /// Validate company has sufficient balance for a fee (throws exception if insufficient)
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="amount">Required amount</param>
    /// <param name="feePurpose">Purpose of the fee (for logging)</param>
    /// <exception cref="AppException">Thrown if wallet not found or insufficient balance</exception>
    Task ValidateCompanyBalanceForFeeAsync(Guid companyId, decimal amount, string feePurpose);
    
    /// <summary>
    /// Get wallet transactions
    /// </summary>
    Task<List<WalletTransactionDto>> GetWalletTransactionsAsync(Guid walletId, int page = 1, int pageSize = 20);
    
    /// <summary>
    /// Get wallet transactions by query
    /// </summary>
    Task<List<WalletTransactionDto>> GetTransactionsByQueryAsync(WalletTransactionQueryDto query);
    
    /// <summary>
    /// Get wallet report
    /// </summary>
    Task<WalletReportDto> GetWalletReportAsync(Guid walletId, DateTime? fromDate = null, DateTime? toDate = null);
    
    /// <summary>
    /// Transfer funds between wallets (e.g., company to SuperAdmin for fees)
    /// </summary>
    Task<bool> TransferFundsAsync(Guid fromWalletId, Guid toWalletId, decimal amount, string description, string? userId);
}
