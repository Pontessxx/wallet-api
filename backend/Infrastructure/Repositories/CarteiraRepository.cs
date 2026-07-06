namespace Infrastructure.Repositories;

public class CarteiraRepository : ICarteiraRepository
{
    private readonly ApplicationDbContext _context;

    public CarteiraRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Carteira?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Carteiras
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Carteira?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => _context.Carteiras
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public Task<List<Carteira>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Carteiras
            .Where(c => c.UserId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(Carteira carteira, CancellationToken ct = default)
        => await _context.Carteiras.AddAsync(carteira, ct);

    public Task DeleteAsync(Carteira carteira, CancellationToken ct = default)
    {
        _context.Carteiras.Remove(carteira);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}