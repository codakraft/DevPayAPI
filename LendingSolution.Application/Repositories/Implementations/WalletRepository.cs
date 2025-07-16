using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

/// <summary>
/// Repository implementation for wallet operations
/// </summary>
public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetWalletByIdAsync(Guid walletId)
    {
        return await _context.Wallets
            .Include(w => w.Company)
            .FirstOrDefaultAsync(w => w.Id == walletId);
    }

    public async Task<Wallet?> GetWalletByCompanyIdAsync(Guid companyId)
    {
        return await _context.Wallets
            .Include(w => w.Company)
            .FirstOrDefaultAsync(w => w.CompanyId == companyId);
    }

    public async Task<Wallet?> GetSuperAdminWalletAsync()
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.IsSuperAdminWallet);
    }

    public async Task<Wallet> CreateWalletAsync(Wallet wallet)
    {
        wallet.Id = Guid.NewGuid();
        wallet.CreatedAt = DateTime.UtcNow;
        wallet.UpdatedAt = DateTime.UtcNow;
        
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> UpdateWalletAsync(Wallet wallet)
    {
        wallet.UpdatedAt = DateTime.UtcNow;
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<List<Wallet>> GetAllWalletsAsync()
    {
        return await _context.Wallets
            .Include(w => w.Company)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> HasSufficientBalanceAsync(Guid walletId, decimal amount)
    {
        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == walletId);
        
        return wallet?.Balance >= amount;
    }
}

/// <summary>
/// Repository implementation for wallet transaction operations
/// </summary>
public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public WalletTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WalletTransaction> CreateTransactionAsync(WalletTransaction transaction)
    {
        transaction.Id = Guid.NewGuid();
        transaction.CreatedAt = DateTime.UtcNow;
        
        _context.WalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<List<WalletTransaction>> GetTransactionsByWalletIdAsync(Guid walletId, int page = 1, int pageSize = 20)
    {
        return await _context.WalletTransactions
            .Include(wt => wt.InitiatedByUser)
            .Where(wt => wt.WalletId == walletId)
            .OrderByDescending(wt => wt.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<WalletTransaction?> GetTransactionByIdAsync(Guid transactionId)
    {
        return await _context.WalletTransactions
            .Include(wt => wt.Wallet)
            .Include(wt => wt.InitiatedByUser)
            .FirstOrDefaultAsync(wt => wt.Id == transactionId);
    }

    public async Task<WalletTransaction?> GetTransactionByPaystackReferenceAsync(string paystackReference)
    {
        return await _context.WalletTransactions
            .Include(wt => wt.Wallet)
            .FirstOrDefaultAsync(wt => wt.PaystackReference == paystackReference);
    }

    public async Task<List<WalletTransaction>> GetTransactionsByQueryAsync(WalletTransactionQueryDto query)
    {
        var queryable = _context.WalletTransactions
            .Include(wt => wt.InitiatedByUser)
            .AsQueryable();

        if (query.WalletId.HasValue)
            queryable = queryable.Where(wt => wt.WalletId == query.WalletId.Value);

        if (query.TransactionType.HasValue)
            queryable = queryable.Where(wt => (int)wt.TransactionType == query.TransactionType.Value);

        if (query.FromDate.HasValue)
            queryable = queryable.Where(wt => wt.CreatedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            queryable = queryable.Where(wt => wt.CreatedAt <= query.ToDate.Value);

        return await queryable
            .OrderByDescending(wt => wt.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
    }

    public async Task<int> GetTransactionCountAsync(Guid walletId)
    {
        return await _context.WalletTransactions
            .CountAsync(wt => wt.WalletId == walletId);
    }
}
