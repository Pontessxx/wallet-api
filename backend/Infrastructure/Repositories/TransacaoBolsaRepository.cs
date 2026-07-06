namespace Infrastructure.Repositories;

public class TransacaoBolsaRepository : ITransacaoBolsaRepository
{
    private readonly ApplicationDbContext _context;

    public TransacaoBolsaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<TransacaoBolsa?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.TransacoesBolsa
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<List<TransacaoBolsa>> GetByContaCarteiraIdAsync(Guid contaCarteiraId, CancellationToken ct = default)
        => _context.TransacoesBolsa
            .Where(t => t.ContaCarteiraId == contaCarteiraId)
            .OrderByDescending(t => t.DataLancamento)
            .ToListAsync(ct);

    public async Task AddAsync(TransacaoBolsa transacaoBolsa, CancellationToken ct = default)
        => await _context.TransacoesBolsa.AddAsync(transacaoBolsa, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}