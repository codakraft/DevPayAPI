using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LendingSolution.Application.Repositories.Implementations;

public class EWalletRepository : IEWalletRepository
{
    private readonly ApplicationDbContext _context;

    public EWalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EWallet> CreateAsync(EWallet eWallet)
    {
        _context.EWallets.Add(eWallet);
        await _context.SaveChangesAsync();
        return eWallet;
    }

    public async Task<EWallet> UpdateAsync(EWallet eWallet)
    {
        eWallet.UpdatedAt = DateTime.UtcNow;
        _context.EWallets.Update(eWallet);
        await _context.SaveChangesAsync();
        return eWallet;
    }

    public async Task<EWallet?> GetByMobileNumberAsync(string mobileNumber)
    {
        return await _context.EWallets
            .FirstOrDefaultAsync(w => w.MobileNumber == mobileNumber);
    }

    public async Task<EWallet?> GetByEmbedlyCustomerIdAsync(string embedlyCustomerId)
    {
        return await _context.EWallets
            .FirstOrDefaultAsync(w => w.EmbedlyCustomerId == embedlyCustomerId);
    }

    public async Task<EWallet?> GetByEmbedlyWalletIdAsync(string embedlyWalletId)
    {
        return await _context.EWallets
            .FirstOrDefaultAsync(w => w.EmbedlyWalletId == embedlyWalletId);
    }

    public async Task<EWallet?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.EWallets
            .FirstOrDefaultAsync(w => w.AccountNumber == accountNumber);
    }
}
