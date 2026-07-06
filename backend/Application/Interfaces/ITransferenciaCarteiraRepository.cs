namespace Application.Interfaces;

public interface ITransferenciaCarteiraRepository
{
    Task<TransferenciaCarteira?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TransferenciaCarteira>> GetByContaCarteiraIdAsync(Guid contaCarteiraId, CancellationToken ct = default);
    Task AddAsync(TransferenciaCarteira transferenciaCarteira, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}