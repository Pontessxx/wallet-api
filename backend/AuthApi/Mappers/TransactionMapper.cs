namespace AuthApi.Mappers;

public static class TransactionMapper
{
    public static TransferenciaCarteira ToEntity(this CreateTransferRequest request)
        => new()
        {
            Id = Guid.NewGuid(),
            CarteiraOrigemId = request.CarteiraId,
            CarteiraDestinoId = request.CarteiraDestinoId,
            Valor = request.Valor,
            Encargos = request.Encargos,
            ValorTotal = request.Valor + request.Encargos,
            Efetivada = request.Efetivada,
            DataLancamento = request.DataLancamento,
            DataVencimento = request.DataVencimento,
            DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null,
            Observacoes = request.Observacoes,
            CriadaEm = DateTime.UtcNow
        };

    public static void ApplyUpdate(this TransferenciaCarteira transferencia, TransferUpsertRequest request)
    {
        transferencia.CarteiraOrigemId = request.CarteiraId;
        transferencia.CarteiraDestinoId = request.CarteiraDestinoId;
        transferencia.Valor = request.Valor;
        transferencia.Encargos = request.Encargos;
        transferencia.ValorTotal = request.Valor + request.Encargos;
        transferencia.Efetivada = request.Efetivada;
        transferencia.DataLancamento = request.DataLancamento;
        transferencia.DataVencimento = request.DataVencimento;
        transferencia.DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null;
        transferencia.Observacoes = request.Observacoes;
        transferencia.AtualizadaEm = DateTime.UtcNow;
    }

    public static TransacaoBolsa ToEntity(this CreateExchangeRequest request)
    {
        var valor = request.Quantidade * request.PrecoUnitario;
        return new()
        {
            Id = Guid.NewGuid(),
            CarteiraId = request.CarteiraId,
            CodigoAtivo = request.CodigoAtivo.Trim().ToUpperInvariant(),
            Lado = request.Lado!.Value,
            Quantidade = request.Quantidade,
            PrecoUnitario = request.PrecoUnitario,
            Valor = valor,
            Encargos = request.Encargos,
            ValorTotal = valor + request.Encargos,
            Efetivada = request.Efetivada,
            DataLancamento = request.DataLancamento,
            DataVencimento = request.DataVencimento,
            DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null,
            Observacoes = request.Observacoes,
            CriadaEm = DateTime.UtcNow
        };
    }

    public static void ApplyUpdate(this TransacaoBolsa exchange, ExchangeUpsertRequest request)
    {
        var valor = request.Quantidade * request.PrecoUnitario;
        exchange.CarteiraId = request.CarteiraId;
        exchange.CodigoAtivo = request.CodigoAtivo.Trim().ToUpperInvariant();
        exchange.Lado = request.Lado!.Value;
        exchange.Quantidade = request.Quantidade;
        exchange.PrecoUnitario = request.PrecoUnitario;
        exchange.Valor = valor;
        exchange.Encargos = request.Encargos;
        exchange.ValorTotal = valor + request.Encargos;
        exchange.Efetivada = request.Efetivada;
        exchange.DataLancamento = request.DataLancamento;
        exchange.DataVencimento = request.DataVencimento;
        exchange.DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null;
        exchange.Observacoes = request.Observacoes;
        exchange.AtualizadaEm = DateTime.UtcNow;
    }

    public static Transacoes ToEntity(this CreateEntryRequest request)
        => new()
        {
            Id = Guid.NewGuid(),
            CarteiraId = request.CarteiraId,
            Tipo = request.Tipo,
            Categoria = request.Categoria,
            Valor = request.Valor,
            Encargos = request.Encargos,
            ValorTotal = request.Valor + request.Encargos,
            Efetivada = request.Efetivada,
            DataLancamento = request.DataLancamento,
            DataVencimento = request.DataVencimento,
            DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null,
            Observacoes = request.Observacoes,
            CriadaEm = DateTime.UtcNow
        };

    public static void ApplyUpdate(this Transacoes transacao, EntryUpsertRequest request)
    {
        transacao.CarteiraId = request.CarteiraId;
        transacao.Tipo = request.Tipo;
        transacao.Categoria = request.Categoria;
        transacao.Valor = request.Valor;
        transacao.Encargos = request.Encargos;
        transacao.ValorTotal = request.Valor + request.Encargos;
        transacao.Efetivada = request.Efetivada;
        transacao.DataLancamento = request.DataLancamento;
        transacao.DataVencimento = request.DataVencimento;
        transacao.DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null;
        transacao.Observacoes = request.Observacoes;
        transacao.AtualizadaEm = DateTime.UtcNow;
    }
}
