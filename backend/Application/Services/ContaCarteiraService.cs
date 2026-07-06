namespace Application.Services;

public class ContaCarteiraService
{
    private readonly IContaCarteiraRepository _contaCarteiraRepository;
    private readonly ICarteiraRepository _carteiraRepository;

    public ContaCarteiraService(
        IContaCarteiraRepository contaCarteiraRepository,
        ICarteiraRepository carteiraRepository)
    {
        _contaCarteiraRepository = contaCarteiraRepository;
        _carteiraRepository = carteiraRepository;
    }

    public Task<List<ContaCarteira>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _contaCarteiraRepository.GetByUserIdAsync(userId, ct);

    public Task<ContaCarteira?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _contaCarteiraRepository.GetByIdAsync(id, ct);

    public async Task<ContaCarteira> CreateAsync(
        Guid userId,
        string nome,
        WalletCategory categoria,
        string descricao,
        decimal saldoInicial,
        CancellationToken ct = default)
    {
        var contaCarteira = new ContaCarteira
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Nome = nome,
            Categoria = categoria
        };

        var carteira = new Carteira
        {
            Id = Guid.NewGuid(),
            ContaCarteiraId = contaCarteira.Id,
            Descricao = descricao,
            SaldoInicial = saldoInicial,
            Receitas = 0,
            Despesas = 0,
            Transferencias = 0,
            Saldo = saldoInicial,
            SaldoProjetado = saldoInicial
        };

        await _contaCarteiraRepository.AddAsync(contaCarteira, ct);
        await _carteiraRepository.AddAsync(carteira, ct);
        await _contaCarteiraRepository.SaveChangesAsync(ct);

        contaCarteira.Carteira = carteira;
        return contaCarteira;
    }

    public async Task UpdateCarteiraAsync(Guid contaCarteiraId, Guid userId, string descricao, CancellationToken ct = default)
    {
        var contaCarteira = await _contaCarteiraRepository.GetByIdAsync(contaCarteiraId, ct)
            ?? throw new KeyNotFoundException("Conta carteira não encontrada.");

        if (contaCarteira.UserId != userId)
            throw new UnauthorizedAccessException("Você não tem permissão para alterar esta carteira.");

        contaCarteira.Carteira.Descricao = descricao;
        await _carteiraRepository.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var contaCarteira = await _contaCarteiraRepository.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException("Conta carteira não encontrada.");

        if (contaCarteira.UserId != userId)
            throw new UnauthorizedAccessException("Você não tem permissão para excluir esta conta.");

        await _contaCarteiraRepository.DeleteAsync(contaCarteira, ct);
        await _contaCarteiraRepository.SaveChangesAsync(ct);
    }
}
