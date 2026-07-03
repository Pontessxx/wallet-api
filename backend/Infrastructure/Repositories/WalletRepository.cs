namespace Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<Wallet?> GetByWalletAccountIdAsync(Guid walletAccountId, CancellationToken ct = default)
        => _context.Wallets
            .FirstOrDefaultAsync(w => w.WalletAccountId == walletAccountId, ct);

    public async Task AddAsync(Wallet wallet, CancellationToken ct = default)
        => await _context.Wallets.AddAsync(wallet, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}