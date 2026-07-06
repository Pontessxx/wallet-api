namespace Infrastructure.Repositories;

public class TransferenciaCarteiraRepository : ITransferenciaCarteiraRepository
{
    private readonly ApplicationDbContext _context;

    public TransferenciaCarteiraRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<TransferenciaCarteira?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.TransferenciasCarteira
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<List<TransferenciaCarteira>> GetByContaCarteiraIdAsync(Guid contaCarteiraId, CancellationToken ct = default)
        => _context.TransferenciasCarteira
            .Where(t => t.ContaCarteiraOrigemId == contaCarteiraId || t.ContaCarteiraDestinoId == contaCarteiraId)
            .OrderByDescending(t => t.DataLancamento)
            .ToListAsync(ct);

    public async Task AddAsync(TransferenciaCarteira transferenciaCarteira, CancellationToken ct = default)
        => await _context.TransferenciasCarteira.AddAsync(transferenciaCarteira, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}