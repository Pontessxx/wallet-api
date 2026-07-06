namespace Infrastructure.Repositories;

public class CarteiraRepository : ICarteiraRepository
{
    private readonly ApplicationDbContext _context;

    public CarteiraRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Carteira?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Wallets
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<Carteira?> GetByWalletAccountIdAsync(Guid walletAccountId, CancellationToken ct = default)
        => _context.Wallets
            .FirstOrDefaultAsync(w => w.ContaCarteiraId == walletAccountId, ct);

    public async Task AddAsync(Carteira carteira, CancellationToken ct = default)
        => await _context.Wallets.AddAsync(carteira, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}