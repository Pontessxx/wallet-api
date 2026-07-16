namespace AuthApi.Controllers.V2;

[ApiController]
[Route("transfer/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class TransferController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public TransferController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Cria uma nova transação para uma carteira do usuário autenticado.
    /// </summary>
    /// <param name="request">Dados da transação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação criada</returns>
    /// <response code="201">Transação criada com sucesso</response>
    /// <response code="400">Dados inválidos da transação</response>
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

        var category = await GetOwnedCategoryAsync(userId, request.CategoriaId, ct);
        if (category is null)
            return this.BadRequestError("Categoria informada não pertence ao usuário autenticado.");

        var transacao = request.ToEntity();
        transacao.Categoria = category;

        await _dbContext.Transacoes.AddAsync(transacao, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapTransaction(transacao));
    }

    /// <summary>
    /// Atualiza uma transação existente do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="request">Novos dados da transação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação atualizada</returns>
    /// <response code="200">Transação atualizada com sucesso</response>
    /// <response code="400">Dados inválidos da transação</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpPut("edit")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntry(
        [FromQuery] Guid id,
        [FromBody] UpdateEntryRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        var category = await GetOwnedCategoryAsync(userId, request.CategoriaId, ct);
        if (category is null)
            return this.BadRequestError("Categoria informada não pertence ao usuário autenticado.");

        var transacao = await _dbContext.Transacoes
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return this.NotFoundError("Transação não encontrada.");

        transacao.ApplyUpdate(request);
        transacao.Categoria = category;

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapTransaction(transacao));
    }

    /// <summary>
    /// Retorna uma transação específica do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação encontrada</returns>
    /// <response code="200">Transação encontrada com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacao = await _dbContext.Transacoes
            .AsNoTracking()
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return this.NotFoundError("Transação não encontrada.");

        return Ok(MapTransaction(transacao));
    }

    /// <summary>
    /// Retorna o histórico de transações do usuário autenticado para um período.
    /// </summary>
    /// <param name="periodType">Tipo de período: range, monthly ou yearly</param>
    /// <param name="startDate">Data inicial (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="endDate">Data final (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="year">Ano para filtros monthly/yearly</param>
    /// <param name="month">Mês (1..12) para filtro monthly</param>
    /// <param name="transactionTypeFilter">Filtro opcional pelo tipo da transação</param>
    /// <param name="categoriaId">Filtro opcional por categoria</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Histórico de transações</returns>
    /// <response code="200">Histórico retornado com sucesso</response>
    /// <response code="400">Parâmetros de período ou filtros inválidos</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(TransactionHistoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
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

        var transacoesQuery = _dbContext.Transacoes
            .AsNoTracking()
            .Include(t => t.Categoria)
            .Where(t => walletIds.Contains(t.CarteiraId))
            .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive);

        if (transactionTypeFilter.HasValue)
            transacoesQuery = transacoesQuery.Where(t => t.Tipo == transactionTypeFilter.Value);

        if (categoriaId.HasValue)
            transacoesQuery = transacoesQuery.Where(t => t.CategoriaId == categoriaId.Value);

        var historico = await transacoesQuery
            .OrderByDescending(t => t.DataLancamento)
            .Select(t => MapTransaction(t))
            .ToListAsync(ct);

        return Ok(new TransactionHistoryResult(historico));
    }

    /// <summary>
    /// Remove uma transação do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Transação removida com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação não encontrada</response>
    [HttpDelete("remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken ct)
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
            transacao.AtualizadaEm);

    private Task<Category?> GetOwnedCategoryAsync(Guid userId, Guid categoryId, CancellationToken ct)
        => _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, ct);
}
