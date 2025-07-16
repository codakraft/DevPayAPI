using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using Microsoft.Extensions.Logging;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Wallet service implementation for wallet operations
/// </summary>
public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly IPaystackService _paystackService;
    private readonly ILogger<WalletService> _logger;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPaystackService paystackService,
        ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _paystackService = paystackService;
        _logger = logger;
    }

    public async Task<WalletDto?> GetWalletByIdAsync(Guid walletId)
    {
        var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
        return wallet == null ? null : MapToWalletDto(wallet);
    }

    public async Task<WalletDto?> GetWalletByCompanyIdAsync(Guid companyId)
    {
        var wallet = await _walletRepository.GetWalletByCompanyIdAsync(companyId);
        return wallet == null ? null : MapToWalletDto(wallet);
    }

    public async Task<WalletDto?> GetSuperAdminWalletAsync()
    {
        var wallet = await _walletRepository.GetSuperAdminWalletAsync();
        return wallet == null ? null : MapToWalletDto(wallet);
    }

    public async Task<WalletDto> CreateCompanyWalletAsync(Guid companyId)
    {
        var existingWallet = await _walletRepository.GetWalletByCompanyIdAsync(companyId);
        if (existingWallet != null)
        {
            throw new InvalidOperationException("Company already has a wallet");
        }

        var wallet = new Wallet
        {
            CompanyId = companyId,
            Balance = 0,
            TotalCredits = 0,
            TotalDebits = 0,
            IsSuperAdminWallet = false
        };

        var createdWallet = await _walletRepository.CreateWalletAsync(wallet);
        _logger.LogInformation("Created wallet for company: {CompanyId}", companyId);
        
        return MapToWalletDto(createdWallet);
    }

    public async Task<WalletDto> CreateSuperAdminWalletAsync()
    {
        var existingWallet = await _walletRepository.GetSuperAdminWalletAsync();
        if (existingWallet != null)
        {
            throw new InvalidOperationException("SuperAdmin wallet already exists");
        }

        var wallet = new Wallet
        {
            CompanyId = null,
            Balance = 0,
            TotalCredits = 0,
            TotalDebits = 0,
            IsSuperAdminWallet = true
        };

        var createdWallet = await _walletRepository.CreateWalletAsync(wallet);
        _logger.LogInformation("Created SuperAdmin wallet");
        
        return MapToWalletDto(createdWallet);
    }

    public async Task<List<WalletDto>> GetAllWalletsAsync()
    {
        var wallets = await _walletRepository.GetAllWalletsAsync();
        return wallets.Select(MapToWalletDto).ToList();
    }

    public async Task<PaystackInitializationDto> InitiateWalletFundingAsync(FundWalletDto fundWalletDto)
    {
        var wallet = await _walletRepository.GetWalletByIdAsync(fundWalletDto.WalletId);
        if (wallet == null)
        {
            return new PaystackInitializationDto
            {
                Status = false,
                Message = "Wallet not found"
            };
        }

        // Generate unique reference
        var reference = $"WF_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";

        var result = await _paystackService.InitializePaymentAsync(
            fundWalletDto.Email,
            fundWalletDto.Amount,
            reference,
            fundWalletDto.CallbackUrl);

        if (result.Status)
        {
            // Create pending transaction
            var transaction = new WalletTransaction
            {
                WalletId = fundWalletDto.WalletId,
                Amount = fundWalletDto.Amount,
                BalanceAfter = wallet.Balance, // Will be updated when payment is verified
                TransactionType = WalletTransactionType.PaystackFunding,
                Description = $"Wallet funding via Paystack - {fundWalletDto.Amount:C}",
                PaystackReference = reference,
                InitiatedBy = null // Will be set when available
            };

            await _transactionRepository.CreateTransactionAsync(transaction);
            _logger.LogInformation("Initiated wallet funding for wallet: {WalletId}, amount: {Amount}, reference: {Reference}", 
                fundWalletDto.WalletId, fundWalletDto.Amount, reference);
        }

        return result;
    }

    public async Task<bool> CompleteWalletFundingAsync(string paystackReference)
    {
        try
        {
            // Verify payment with Paystack
            var isPaymentValid = await _paystackService.VerifyPaymentAsync(paystackReference);
            if (!isPaymentValid)
            {
                _logger.LogWarning("Paystack payment verification failed for reference: {Reference}", paystackReference);
                return false;
            }

            // Get the pending transaction
            var transaction = await _transactionRepository.GetTransactionByPaystackReferenceAsync(paystackReference);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found for Paystack reference: {Reference}", paystackReference);
                return false;
            }

            // Get wallet and update balance
            var wallet = await _walletRepository.GetWalletByIdAsync(transaction.WalletId);
            if (wallet == null)
            {
                _logger.LogError("Wallet not found for transaction: {TransactionId}", transaction.Id);
                return false;
            }

            // Update wallet balance
            wallet.Balance += transaction.Amount;
            wallet.TotalCredits += transaction.Amount;
            await _walletRepository.UpdateWalletAsync(wallet);

            // Update transaction with final balance
            transaction.BalanceAfter = wallet.Balance;
            await _transactionRepository.CreateTransactionAsync(transaction); // This might need to be an update method

            _logger.LogInformation("Completed wallet funding for wallet: {WalletId}, amount: {Amount}", 
                wallet.Id, transaction.Amount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing wallet funding for reference: {Reference}", paystackReference);
            return false;
        }
    }

    public async Task<bool> DebitWalletAsync(DebitWalletDto debitWalletDto, string userId)
    {
        try
        {
            var wallet = await _walletRepository.GetWalletByIdAsync(debitWalletDto.WalletId);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found: {WalletId}", debitWalletDto.WalletId);
                return false;
            }

            if (wallet.Balance < debitWalletDto.Amount)
            {
                _logger.LogWarning("Insufficient balance in wallet: {WalletId}. Required: {Amount}, Available: {Balance}", 
                    debitWalletDto.WalletId, debitWalletDto.Amount, wallet.Balance);
                return false;
            }

            // Update wallet balance
            wallet.Balance -= debitWalletDto.Amount;
            wallet.TotalDebits += debitWalletDto.Amount;
            await _walletRepository.UpdateWalletAsync(wallet);

            // Create transaction record
            var transaction = new WalletTransaction
            {
                WalletId = debitWalletDto.WalletId,
                Amount = -debitWalletDto.Amount, // Negative for debit
                BalanceAfter = wallet.Balance,
                TransactionType = (WalletTransactionType)debitWalletDto.TransactionType,
                Description = debitWalletDto.Description,
                ReferenceId = debitWalletDto.ReferenceId,
                InitiatedBy = userId
            };

            await _transactionRepository.CreateTransactionAsync(transaction);

            _logger.LogInformation("Debited wallet: {WalletId}, amount: {Amount}", 
                debitWalletDto.WalletId, debitWalletDto.Amount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debiting wallet: {WalletId}", debitWalletDto.WalletId);
            return false;
        }
    }

    public async Task<bool> CreditWalletAsync(Guid walletId, decimal amount, string description, string? referenceId, string userId)
    {
        try
        {
            var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found: {WalletId}", walletId);
                return false;
            }

            // Update wallet balance
            wallet.Balance += amount;
            wallet.TotalCredits += amount;
            await _walletRepository.UpdateWalletAsync(wallet);

            // Create transaction record
            var transaction = new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                BalanceAfter = wallet.Balance,
                TransactionType = WalletTransactionType.Credit,
                Description = description,
                ReferenceId = referenceId,
                InitiatedBy = userId
            };

            await _transactionRepository.CreateTransactionAsync(transaction);

            _logger.LogInformation("Credited wallet: {WalletId}, amount: {Amount}", walletId, amount);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crediting wallet: {WalletId}", walletId);
            return false;
        }
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount)
    {
        return await _walletRepository.HasSufficientBalanceAsync(walletId, amount);
    }

    public async Task<List<WalletTransactionDto>> GetWalletTransactionsAsync(Guid walletId, int page = 1, int pageSize = 20)
    {
        var transactions = await _transactionRepository.GetTransactionsByWalletIdAsync(walletId, page, pageSize);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<List<WalletTransactionDto>> GetTransactionsByQueryAsync(WalletTransactionQueryDto query)
    {
        var transactions = await _transactionRepository.GetTransactionsByQueryAsync(query);
        return transactions.Select(MapToTransactionDto).ToList();
    }

    public async Task<WalletReportDto> GetWalletReportAsync(Guid walletId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
        if (wallet == null)
        {
            throw new ArgumentException("Wallet not found", nameof(walletId));
        }

        var query = new WalletTransactionQueryDto
        {
            WalletId = walletId,
            FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
            ToDate = toDate ?? DateTime.UtcNow,
            PageSize = 10
        };

        var recentTransactions = await GetTransactionsByQueryAsync(query);
        var transactionCount = await _transactionRepository.GetTransactionCountAsync(walletId);

        // Calculate total fees (specific transaction types)
        var feeQuery = new WalletTransactionQueryDto
        {
            WalletId = walletId,
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            PageSize = int.MaxValue
        };
        
        var allTransactions = await _transactionRepository.GetTransactionsByQueryAsync(feeQuery);
        var totalFees = allTransactions
            .Where(t => t.TransactionType == WalletTransactionType.FeeCharge ||
                       t.TransactionType == WalletTransactionType.LoanDisbursementFee ||
                       t.TransactionType == WalletTransactionType.ProcessingFee ||
                       t.TransactionType == WalletTransactionType.ManagementFee ||
                       t.TransactionType == WalletTransactionType.LegalFee)
            .Sum(t => Math.Abs(t.Amount));

        return new WalletReportDto
        {
            WalletId = walletId,
            WalletName = wallet.IsSuperAdminWallet ? "SuperAdmin Wallet" : $"Company Wallet ({wallet.Company?.Name ?? "Unknown"})",
            CurrentBalance = wallet.Balance,
            TotalCredits = wallet.TotalCredits,
            TotalDebits = wallet.TotalDebits,
            TotalFees = totalFees,
            TransactionCount = transactionCount,
            ReportPeriodFrom = query.FromDate ?? DateTime.UtcNow.AddDays(-30),
            ReportPeriodTo = query.ToDate ?? DateTime.UtcNow,
            RecentTransactions = recentTransactions
        };
    }

    public async Task<bool> TransferFundsAsync(Guid fromWalletId, Guid toWalletId, decimal amount, string description, string userId)
    {
        try
        {
            // Debit from source wallet
            var debitSuccess = await DebitWalletAsync(new DebitWalletDto
            {
                WalletId = fromWalletId,
                Amount = amount,
                Description = $"Transfer out: {description}",
                ReferenceId = $"TRF_{Guid.NewGuid():N}",
                TransactionType = (int)WalletTransactionType.Debit
            }, userId);

            if (!debitSuccess)
            {
                return false;
            }

            // Credit to destination wallet
            var creditSuccess = await CreditWalletAsync(toWalletId, amount, $"Transfer in: {description}", 
                $"TRF_{Guid.NewGuid():N}", userId);

            if (!creditSuccess)
            {
                // TODO: Implement rollback mechanism
                _logger.LogError("Failed to credit destination wallet after successful debit. Manual intervention required.");
                return false;
            }

            _logger.LogInformation("Transferred {Amount} from wallet {FromWallet} to wallet {ToWallet}", 
                amount, fromWalletId, toWalletId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring funds from {FromWallet} to {ToWallet}", fromWalletId, toWalletId);
            return false;
        }
    }

    private static WalletDto MapToWalletDto(Wallet wallet)
    {
        return new WalletDto
        {
            Id = wallet.Id,
            CompanyId = wallet.CompanyId,
            CompanyName = wallet.Company?.Name,
            Balance = wallet.Balance,
            TotalCredits = wallet.TotalCredits,
            TotalDebits = wallet.TotalDebits,
            IsSuperAdminWallet = wallet.IsSuperAdminWallet,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    private static WalletTransactionDto MapToTransactionDto(WalletTransaction transaction)
    {
        return new WalletTransactionDto
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            Amount = transaction.Amount,
            BalanceAfter = transaction.BalanceAfter,
            TransactionType = transaction.TransactionType.ToString(),
            Description = transaction.Description,
            ReferenceId = transaction.ReferenceId,
            InitiatedBy = transaction.InitiatedByUser?.Email ?? transaction.InitiatedBy,
            PaystackReference = transaction.PaystackReference,
            CreatedAt = transaction.CreatedAt
        };
    }
}
