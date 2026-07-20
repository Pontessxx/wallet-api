namespace AuthApi.Controllers.V2;

[ApiController]
[Controller]
[Route("history/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class History : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public History(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retorna o histórico unificado de transações e transferências do usuário autenticado para um período.
    /// </summary>
    /// <param name="periodType">Tipo de período: range, monthly ou yearly</param>
    /// <param name="startDate">Data inicial (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="endDate">Data final (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="year">Ano para filtros monthly/yearly</param>
    /// <param name="month">Mês (1..12) para filtro monthly</param>
    /// <param name="transactionTypeFilter">Filtro opcional pelo tipo da transação</param>
    /// <param name="categoriaId">Filtro opcional por categoria (aplicado apenas em transações)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Histórico unificado</returns>
    /// <response code="200">Histórico retornado com sucesso</response>
    /// <response code="400">Parâmetros de período ou filtros inválidos</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(TransactionHistoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTransactionsAndTransfersHistory(
        [FromQuery] string? periodType,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery(Name = "tipo")] TipoTransacoes? transactionTypeFilter,
        [FromQuery] Guid? categoriaId,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacoes>());
            return this.BadRequestError($"Parâmetro de query tipo inválido. Valores permitidos: {allowedValues}.");
        }

        if (!PeriodQueryParser.TryResolveDateRange(
                periodType,
                startDate,
                endDate,
                year,
                month,
                requirePeriodType: true,
                out var rangeStart,
                out var rangeEndExclusive,
                out var periodError))
        {
            return this.BadRequestError(periodError!);
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        List<TransactionResult> transacoes = [];
        List<TransactionResult> transferencias = [];

        var includeTransfers = !transactionTypeFilter.HasValue || transactionTypeFilter == TipoTransacoes.Transferencia;
        var includeTransactions = !transactionTypeFilter.HasValue || transactionTypeFilter != TipoTransacoes.Transferencia;

        if (includeTransactions)
        {
            var transacoesQuery = _dbContext.Transacoes
                .AsNoTracking()
                .Include(t => t.Categoria)
                .Where(t => walletIds.Contains(t.CarteiraId))
                .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive);

            if (transactionTypeFilter.HasValue)
                transacoesQuery = transacoesQuery.Where(t => t.Tipo == transactionTypeFilter.Value);

            if (categoriaId.HasValue)
                transacoesQuery = transacoesQuery.Where(t => t.CategoriaId == categoriaId.Value);

            transacoes = await transacoesQuery
                .Select(t => MapTransaction(t))
                .ToListAsync(ct);
        }

        if (includeTransfers && !categoriaId.HasValue)
        {
            transferencias = await _dbContext.TransferenciasCarteira
                .AsNoTracking()
                .Where(t => walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId))
                .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive)
                .Select(t => MapTransfer(t))
                .ToListAsync(ct);
        }

        var historico = transacoes
            .Concat(transferencias)
            .OrderByDescending(t => t.DataLancamento)
            .ToList();

        return Ok(new TransactionHistoryResult(historico));
    }

    /// <summary>
    /// Retorna o histórico de transações de bolsa do usuário autenticado para um período.
    /// </summary>
    /// <param name="periodType">Tipo de período: range, monthly ou yearly</param>
    /// <param name="startDate">Data inicial (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="endDate">Data final (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="year">Ano para filtros monthly/yearly</param>
    /// <param name="month">Mês (1..12) para filtro monthly</param>
    /// <param name="exchangeTypeFilter">Filtro opcional pelo lado da operação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Histórico de transações de bolsa</returns>
    /// <response code="200">Histórico retornado com sucesso</response>
    /// <response code="400">Parâmetros de período ou filtros inválidos</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("exchange")]
    [ProducesResponseType(typeof(ExchangeHistoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExchangeHistory(
        [FromQuery] string? periodType,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery(Name = "lado")] TipoTransacaoBolsa? exchangeTypeFilter,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return this.BadRequestError($"Parâmetro de query lado inválido. Valores permitidos: {allowedValues}.");
        }

        if (!PeriodQueryParser.TryResolveDateRange(
                periodType,
                startDate,
                endDate,
                year,
                month,
                requirePeriodType: true,
                out var rangeStart,
                out var rangeEndExclusive,
                out var periodError))
        {
            return this.BadRequestError(periodError!);
        }

        var walletIds = await GetUserInvestmentWalletIdsAsync(userId, ct);

        var exchangeQuery = _dbContext.TransacoesBolsa
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraId))
            .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive);

        if (exchangeTypeFilter.HasValue)
            exchangeQuery = exchangeQuery.Where(t => t.Lado == exchangeTypeFilter.Value);

        var historico = await exchangeQuery
            .OrderByDescending(t => t.DataLancamento)
            .Select(t => MapExchange(t))
            .ToListAsync(ct);

        return Ok(new ExchangeHistoryResult(historico));
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }

    private async Task<HashSet<Guid>> GetUserWalletIdsAsync(Guid userId, CancellationToken ct)
    {
        var walletIds = await _dbContext.Carteiras
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        return walletIds.ToHashSet();
    }

    private async Task<HashSet<Guid>> GetUserInvestmentWalletIdsAsync(Guid userId, CancellationToken ct)
    {
        var walletIds = await _dbContext.Carteiras
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.Categoria == WalletCategory.Investimento)
            .Select(c => c.Id)
            .ToListAsync(ct);

        return walletIds.ToHashSet();
    }

    private static TransactionResult MapTransaction(Transacoes transacao)
        => new(
            transacao.Id,
            transacao.CarteiraId,
            null,
            transacao.Tipo,
            transacao.CategoriaId,
            transacao.Categoria?.Nome,
            transacao.Valor,
            transacao.Encargos,
            transacao.ValorTotal,
            transacao.Efetivada,
            transacao.DataLancamento,
            transacao.DataVencimento,
            transacao.DataEfetivacao,
            transacao.Observacoes,
            transacao.CriadaEm,
            transacao.AtualizadaEm,
            transacao.ObjetivoId);

    private static TransactionResult MapTransfer(TransferenciaCarteira transferencia)
        => new(
            transferencia.Id,
            transferencia.CarteiraOrigemId,
            transferencia.CarteiraDestinoId,
            TipoTransacoes.Transferencia,
            null,
            null,
            transferencia.Valor,
            transferencia.Encargos,
            transferencia.ValorTotal,
            transferencia.Efetivada,
            transferencia.DataLancamento,
            transferencia.DataVencimento,
            transferencia.DataEfetivacao,
            transferencia.Observacoes,
            transferencia.CriadaEm,
            transferencia.AtualizadaEm,
            null);

    private static ExchangeTransactionResult MapExchange(TransacaoBolsa transacao)
        => new(
            transacao.Id,
            transacao.CarteiraId,
            transacao.CodigoAtivo,
            transacao.Lado,
            transacao.Quantidade,
            transacao.PrecoUnitario,
            transacao.Valor,
            transacao.Encargos,
            transacao.ValorTotal,
            transacao.Efetivada,
            transacao.DataLancamento,
            transacao.DataVencimento,
            transacao.DataEfetivacao,
            transacao.Observacoes,
            transacao.CriadaEm,
            transacao.AtualizadaEm);
}
