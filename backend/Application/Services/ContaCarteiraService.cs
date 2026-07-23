namespace Application.Services;

public class ContaCarteiraService
{
    private readonly ICarteiraRepository _carteiraRepository;

    public ContaCarteiraService(ICarteiraRepository carteiraRepository)
    {
        _carteiraRepository = carteiraRepository;
    }

    public async Task<WalletAccountsResult> GetAllAsync(Guid userId, CancellationToken ct = default)
    {
        var carteiras = await _carteiraRepository.GetByUserIdAsync(userId, ct);
        var carteirasResult = carteiras.Select(c => c.ToResult()).ToList();
        return new WalletAccountsResult(carteirasResult);
    }

    public async Task<CarteiraResult> CreateAsync(Guid userId, string nome, WalletCategory categoria, WalletOrigin origem, decimal saldoInicial, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new InvalidOperationException("Nome da carteira é obrigatório.");

        var carteira = new Carteira
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Nome = nome.Trim(),
            Categoria = categoria,
            Origem = origem,
            SaldoInicial = saldoInicial,
            Receitas = 0m,
            Despesas = 0m,
            Transferencias = 0m,
            Saldo = saldoInicial,
            SaldoProjetado = saldoInicial
        };

        await _carteiraRepository.AddAsync(carteira, ct);
        await _carteiraRepository.SaveChangesAsync(ct);

        return carteira.ToResult();
    }

    public async Task<CarteiraResult> UpdateAsync(Guid userId, Guid carteiraId, string nome, WalletCategory categoria, WalletOrigin origem, CancellationToken ct = default)
    {
        var carteira = await _carteiraRepository.GetByIdAndUserIdAsync(carteiraId, userId, ct)
            ?? throw new InvalidOperationException("Carteira não encontrada.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new InvalidOperationException("Nome da carteira é obrigatório.");

        carteira.Nome = nome.Trim();
        carteira.Categoria = categoria;
        carteira.Origem = origem;

        await _carteiraRepository.SaveChangesAsync(ct);

        return carteira.ToResult();
    }

    public async Task DeleteAsync(Guid userId, Guid carteiraId, CancellationToken ct = default)
    {
        var carteira = await _carteiraRepository.GetByIdAndUserIdAsync(carteiraId, userId, ct)
            ?? throw new InvalidOperationException("Carteira não encontrada.");

        await _carteiraRepository.DeleteAsync(carteira, ct);
        await _carteiraRepository.SaveChangesAsync(ct);
    }

    public async Task<WalletSummaryResult> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var carteiras = await _carteiraRepository.GetByUserIdAsync(userId, ct);
        var carteirasResult = carteiras.Select(c => c.ToResult()).ToList();
        var saldoTotal = carteirasResult.Sum(c => c.Saldo);

        return new WalletSummaryResult(carteirasResult, saldoTotal);
    }
}
