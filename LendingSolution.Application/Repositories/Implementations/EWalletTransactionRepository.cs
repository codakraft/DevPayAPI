using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class EWalletTransactionRepository : IEWalletTransactionRepository
{
    private readonly ApplicationDbContext _context;

    public EWalletTransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EWalletTransaction> CreateAsync(EWalletTransaction transaction)
    {
        _context.EWalletTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<EWalletTransaction?> GetByReferenceAsync(string transactionReference)
    {
        return await _context.EWalletTransactions
            .FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);
    }

    public async Task<List<EWalletTransaction>> GetByAccountNumberAsync(string accountNumber, int page = 1, int pageSize = 20)
    {
        return await _context.EWalletTransactions
            .Where(t => t.FromAccount == accountNumber || t.ToAccount == accountNumber)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
