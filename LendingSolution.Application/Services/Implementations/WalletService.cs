using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Application.Exceptions;
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
    private readonly IAuditService _auditService;

    public WalletService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        IPaystackService paystackService,
        ILogger<WalletService> logger,
        IAuditService auditService)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _paystackService = paystackService;
        _logger = logger;
        _auditService = auditService;
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
            throw new AppException("Company already has a wallet");
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

        return MapToWalletDto(createdWallet);
    }

    public async Task<WalletDto> CreateSuperAdminWalletAsync()
    {
        var existingWallet = await _walletRepository.GetSuperAdminWalletAsync();
        if (existingWallet != null)
        {
            throw new AppException("SuperAdmin wallet already exists");
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
            throw new AppException("Wallet not found");
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
        }

        return result;
    }

    public async Task<bool> CompleteWalletFundingAsync(string paystackReference)
    {
        // Verify payment with Paystack
        var isPaymentValid = await _paystackService.VerifyPaymentAsync(paystackReference);
        if (!isPaymentValid)
        {
            return false;
        }

        // Get the pending transaction
        var transaction = await _transactionRepository.GetTransactionByPaystackReferenceAsync(paystackReference);
        if (transaction == null)
        {
            return false;
        }

        // Get wallet and update balance
        var wallet = await _walletRepository.GetWalletByIdAsync(transaction.WalletId);
        if (wallet == null)
        {
            return false;
        }

        // Update wallet balance
        wallet.Balance += transaction.Amount;
        wallet.TotalCredits += transaction.Amount;
        await _walletRepository.UpdateWalletAsync(wallet);

        // Update transaction with final balance
        transaction.BalanceAfter = wallet.Balance;
        await _transactionRepository.CreateTransactionAsync(transaction); // This might need to be an update method

        return true;
    }

    public async Task<bool> DebitWalletAsync(DebitWalletDto debitWalletDto, string? userId)
    {
        var wallet = await _walletRepository.GetWalletByIdAsync(debitWalletDto.WalletId);
        if (wallet == null)
        {
            throw new AppException("Insufficient wallet balance");
        }

        if (wallet.Balance < debitWalletDto.Amount)
        {
            throw new AppException("Insufficient wallet balance");
        }

        var oldBalance = wallet.Balance;

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

        // Audit log
        await _auditService.LogAsync(
            action: "WalletDebited",
            category: AuditCategories.Financial,
            userId: userId,
            entityType: "Wallet",
            entityId: debitWalletDto.WalletId.ToString(),
            companyId: wallet.CompanyId,
            details: debitWalletDto.Description,
            amount: debitWalletDto.Amount,
            oldBalance: oldBalance,
            newBalance: wallet.Balance
        );

        return true;
    }

    public async Task<bool> CreditWalletAsync(Guid walletId, decimal amount, string description, string? referenceId, string? userId)
    {
        var wallet = await _walletRepository.GetWalletByIdAsync(walletId);
        if (wallet == null)
        {
            return false;
        }

        var oldBalance = wallet.Balance;

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

        // Audit log
        await _auditService.LogAsync(
            action: "WalletCredited",
            category: AuditCategories.Financial,
            userId: userId,
            entityType: "Wallet",
            entityId: walletId.ToString(),
            companyId: wallet.CompanyId,
            details: description,
            amount: amount,
            oldBalance: oldBalance,
            newBalance: wallet.Balance
        );

        return true;
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount)
    {
        return await _walletRepository.HasSufficientBalanceAsync(walletId, amount);
    }

    public async Task ValidateCompanyBalanceForFeeAsync(Guid companyId, decimal amount, string feePurpose)
    {
        try
        {
            // Skip if amount is 0 or negative
            if (amount <= 0)
            {
                _logger.LogInformation("Fee amount is {Amount}, skipping wallet validation for Company ID: {CompanyId}", amount, companyId);
                return;
            }

            // Get company wallet
            var companyWallet = await GetWalletByCompanyIdAsync(companyId);
            if (companyWallet == null)
            {
                _logger.LogWarning("Company (ID: {CompanyId}) does not have a wallet. Unable to process {FeePurpose}.", 
                    companyId, feePurpose);
                throw new AppException(
                    "Wallet not found. Please contact support.", 
                    404);
            }

            // Check if company has sufficient balance
            var hasSufficientBalance = await HasSufficientBalanceAsync(companyWallet.Id, amount);
            if (!hasSufficientBalance)
            {
                _logger.LogWarning(
                    "Insufficient wallet balance for Company: {CompanyName} (ID: {CompanyId}). " +
                    "Required: {RequiredAmount:C}, Current Balance: {CurrentBalance:C}. " +
                    "Purpose: {FeePurpose}",
                    companyWallet.CompanyName, companyId, amount, companyWallet.Balance, feePurpose);
                
                throw new AppException(
                    $"Insufficient wallet balance. Required: {amount:C}, Available: {companyWallet.Balance:C}. Please fund your wallet.", 
                    400);
            }

            _logger.LogDebug(
                "Wallet balance validated successfully for Company: {CompanyName}. Balance: {Balance:C}, Required: {Required:C}, Purpose: {Purpose}",
                companyWallet.CompanyName, companyWallet.Balance, amount, feePurpose);
        }
        catch (AppException)
        {
            // Re-throw application exceptions
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error validating wallet balance for Company ID: {CompanyId}, Purpose: {FeePurpose}",
                companyId, feePurpose);
            throw new AppException(
                "Error validating wallet balance. Please try again later.", 
                500);
        }
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
                       t.TransactionType == WalletTransactionType.MaintenanceFee ||
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

    public async Task<bool> TransferFundsAsync(Guid fromWalletId, Guid toWalletId, decimal amount, string description, string? userId)
    {
        var fromWallet = await _walletRepository.GetWalletByIdAsync(fromWalletId);
        var toWallet = await _walletRepository.GetWalletByIdAsync(toWalletId);

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
            return false;
        }

        // Audit log for transfer (especially important for fee transfers)
        var isFeeTransfer = description.Contains("fee", StringComparison.OrdinalIgnoreCase);
        await _auditService.LogAsync(
            action: isFeeTransfer ? "FeeTransferred" : "FundsTransferred",
            category: AuditCategories.Financial,
            userId: userId,
            entityType: "WalletTransfer",
            entityId: $"{fromWalletId}→{toWalletId}",
            companyId: fromWallet?.CompanyId,
            details: $"Transfer from {fromWallet?.Company?.Name ?? "Unknown"} to {toWallet?.Company?.Name ?? (toWallet?.IsSuperAdminWallet == true ? "SuperAdmin" : "Unknown")}: {description}",
            amount: amount
        );

        return true;
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
