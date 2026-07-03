namespace Infrastructure.Repositories;

public class WalletAccountRepository : IWalletAccountRepository
{
    private readonly ApplicationDbContext _context;

    public WalletAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<WalletAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.WalletAccounts
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<List<WalletAccount>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.WalletAccounts
            .Where(w => w.UserId == userId)
            .ToListAsync(ct);

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
        => _context.WalletAccounts
            .AnyAsync(w => w.Id == id, ct);

    public async Task AddAsync(WalletAccount walletAccount, CancellationToken ct = default)
        => await _context.WalletAccounts.AddAsync(walletAccount, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}