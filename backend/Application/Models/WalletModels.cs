namespace Application.Models;

public record CarteiraResult(
    Guid Id,
    string Nome,
    WalletCategory Categoria,
    decimal SaldoInicial,
    decimal Receitas,
    decimal Despesas,
    decimal Transferencias,
    decimal Saldo,
    decimal SaldoProjetado);

public abstract record WalletBaseResult(
    List<CarteiraResult> Carteiras);

public record WalletAccountsResult(
    List<CarteiraResult> Carteiras)
    : WalletBaseResult(Carteiras);

public record WalletSummaryResult(
    List<CarteiraResult> Carteiras,
    decimal SaldoTotal)
    : WalletBaseResult(Carteiras);