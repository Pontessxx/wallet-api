namespace Application.Interfaces;

public interface ICarteiraRepository
{
    Task<Carteira?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Carteira?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<List<Carteira>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Carteira carteira, CancellationToken ct = default);
    Task DeleteAsync(Carteira carteira, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}