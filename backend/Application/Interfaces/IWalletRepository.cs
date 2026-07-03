namespace Application.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Wallet?> GetByWalletAccountIdAsync(Guid walletAccountId, CancellationToken ct = default);
    Task AddAsync(Wallet wallet, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}