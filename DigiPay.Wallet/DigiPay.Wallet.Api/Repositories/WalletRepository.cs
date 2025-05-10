using DigiPay.Wallet.Api.Data;
using DigiPay.Wallet.Api.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;

namespace DigiPay.Wallet.Api.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Models.Wallet?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task<bool> UserHasWalletAsync(Guid userId)
    {
        return await _context.Wallets.AnyAsync(w => w.UserId == userId);
    }

    public async Task AddAsync(Models.Wallet wallet)
    {
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Models.Wallet wallet)
    {
        _context.Wallets.Update(wallet);
        await _context.SaveChangesAsync();
    }
}