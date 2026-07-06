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

    public Task<List<TransferenciaCarteira>> GetByCarteiraIdAsync(Guid carteiraId, CancellationToken ct = default)
        => _context.TransferenciasCarteira
            .Where(t => t.CarteiraOrigemId == carteiraId || t.CarteiraDestinoId == carteiraId)
            .OrderByDescending(t => t.DataLancamento)
            .ToListAsync(ct);

    public async Task AddAsync(TransferenciaCarteira transferenciaCarteira, CancellationToken ct = default)
        => await _context.TransferenciasCarteira.AddAsync(transferenciaCarteira, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}