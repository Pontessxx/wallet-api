namespace Application.Interfaces;

public interface IWalletAccountRepository
{
    Task<WalletAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<WalletAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(WalletAccount walletAccount, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}