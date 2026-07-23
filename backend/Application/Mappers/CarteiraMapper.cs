namespace Application.Mappers;

public static class CarteiraMapper
{
    public static CarteiraResult ToResult(this Carteira carteira)
    {
        var saldoAtual = carteira.SaldoInicial + carteira.Receitas - carteira.Despesas + carteira.Transferencias;

        return new(
            carteira.Id,
            carteira.Nome,
            carteira.Categoria,
            carteira.Origem,
            carteira.SaldoInicial,
            carteira.Receitas,
            carteira.Despesas,
            carteira.Transferencias,
            saldoAtual,
            carteira.SaldoProjetado);
    }
}