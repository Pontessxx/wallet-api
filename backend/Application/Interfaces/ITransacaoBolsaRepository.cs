namespace Application.Interfaces;

public interface ITransacaoBolsaRepository
{
    Task<TransacaoBolsa?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<TransacaoBolsa>> GetByContaCarteiraIdAsync(Guid contaCarteiraId, CancellationToken ct = default);
    Task AddAsync(TransacaoBolsa transacaoBolsa, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}