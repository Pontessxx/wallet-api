namespace Infrastructure.Repositories;

public class ContaCarteiraRepository : IContaCarteiraRepository
{
    private readonly ApplicationDbContext _context;

    public ContaCarteiraRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ContaCarteira?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.WalletAccounts
            .Include(w => w.Carteira)
            .FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<List<ContaCarteira>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.WalletAccounts
            .Include(w => w.Carteira)
            .Where(w => w.UserId == userId)
            .ToListAsync(ct);

    public Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default)
        => _context.WalletAccounts
            .AnyAsync(w => w.Id == id, ct);

    public async Task AddAsync(ContaCarteira contaCarteira, CancellationToken ct = default)
        => await _context.WalletAccounts.AddAsync(contaCarteira, ct);

    public Task DeleteAsync(ContaCarteira contaCarteira, CancellationToken ct = default)
    {
        _context.WalletAccounts.Remove(contaCarteira);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}