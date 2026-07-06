namespace Application.Interfaces;

public interface ICarteiraRepository
{
    Task<Carteira?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Carteira?> GetByWalletAccountIdAsync(Guid walletAccountId, CancellationToken ct = default);
    Task AddAsync(Carteira carteira, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}