namespace DigiPay.Wallet.Api.Repositories.Contracts;
public interface IWalletRepository
{
    Task<Models.Wallet?> GetByUserIdAsync(Guid userId);
    Task<bool> UserHasWalletAsync(Guid userId);
    Task AddAsync(Models.Wallet wallet);
    Task UpdateAsync(Models.Wallet wallet);
}