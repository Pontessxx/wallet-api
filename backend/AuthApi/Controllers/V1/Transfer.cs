namespace AuthApi.Controllers;

[ApiController]
[Route("transfer/v1")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
public class TransferController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public TransferController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retorna uma transação (receita ou despesa) pelo ID.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação encontrada</returns>
    /// <response code="200">Transação encontrada com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpGet("list/{id:guid}")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacao = await _dbContext.Transacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return this.NotFoundError("Transação não encontrada.");

        return Ok(MapTransaction(transacao));
    }

    /// <summary>
    /// Cria uma nova transação (receita ou despesa) na carteira do usuário autenticado.
    /// </summary>
    /// <param name="request">Dados da transação (carteira, tipo, categoria, valor e encargos)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação criada</returns>
    /// <response code="201">Transação criada com sucesso</response>
    /// <response code="400">Dados inválidos para a transação</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpPost("new")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEntry(
        [FromBody] CreateEntryRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        var transacao = request.ToEntity();

        await _dbContext.Transacoes.AddAsync(transacao, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapTransaction(transacao));
    }

    /// <summary>
    /// Atualiza uma transação existente.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="request">Novos dados da transação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação atualizada</returns>
    /// <response code="200">Transação atualizada com sucesso</response>
    /// <response code="400">Dados inválidos para a transação</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpPut("edit/{id:guid}")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntry(
        Guid id,
        [FromBody] UpdateEntryRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        var transacao = await _dbContext.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return this.NotFoundError("Transação não encontrada.");

        transacao.ApplyUpdate(request);

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapTransaction(transacao));
    }

    /// <summary>
    /// Retorna o histórico de transações do usuário autenticado.
    /// </summary>
    /// <param name="transactionTypeFilter">Filtro opcional por tipo de transação (Receita ou Despesa)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista de transações ordenada por data de lançamento</returns>
    /// <response code="200">Histórico retornado com sucesso</response>
    /// <response code="400">Parâmetro de filtro inválido</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(TransactionHistoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery(Name = "tipo")] TipoTransacoes? transactionTypeFilter,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacoes>());
            return this.BadRequestError($"Parâmetro de query tipo inválido. Valores permitidos: {allowedValues}.");
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacoesQuery = _dbContext.Transacoes
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraId));

        if (transactionTypeFilter.HasValue)
            transacoesQuery = transacoesQuery.Where(t => t.Tipo == transactionTypeFilter.Value);

        var historico = await transacoesQuery
            .OrderByDescending(t => t.DataLancamento)
            .Select(t => MapTransaction(t))
            .ToListAsync(ct);

        return Ok(new TransactionHistoryResult(historico));
    }

    /// <summary>
    /// Remove uma transação pelo ID.
    /// </summary>
    /// <param name="id">ID da transação a ser removida</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Transação removida com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpDelete("remove/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacao = await _dbContext.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return this.NotFoundError("Transação não encontrada.");

        _dbContext.Transacoes.Remove(transacao);
        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
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

    private static TransactionResult MapTransaction(Transacoes transacao)
        => new(
            transacao.Id,
            transacao.CarteiraId,
            null,
            transacao.Tipo,
            transacao.Categoria,
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
