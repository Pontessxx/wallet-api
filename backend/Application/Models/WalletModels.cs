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

public record WalletSummaryResult(
    List<CarteiraResult> Carteiras,
    decimal SaldoTotal);