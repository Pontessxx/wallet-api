namespace Application.Interfaces;

public interface IContaCarteiraRepository
{
    Task<ContaCarteira?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<ContaCarteira>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(ContaCarteira contaCarteira, CancellationToken ct = default);
    Task DeleteAsync(ContaCarteira contaCarteira, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}