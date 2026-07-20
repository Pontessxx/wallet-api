namespace AuthApi.Controllers.V2;

[ApiController]
[Controller]
[Route("transaction/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class Transaction : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public Transaction(ApplicationDbContext dbContext)
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

        var goalError = await ValidateGoalLinkAsync(userId, request.Tipo, request.ObjetivoId, ct);
        if (goalError is not null)
            return this.BadRequestError(goalError);

        var transacao = request.ToEntity();
        transacao.Categoria = category;

        await _dbContext.Transacoes.AddAsync(transacao, ct);

        if (request.ObjetivoId.HasValue && transacao.Efetivada)
            await _dbContext.ObjetivoAportes.AddAsync(BuildAporteFromTransacao(request.ObjetivoId.Value, transacao), ct);

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

        var goalError = await ValidateGoalLinkAsync(userId, request.Tipo, request.ObjetivoId, ct);
        if (goalError is not null)
            return this.BadRequestError(goalError);

        var transacao = await _dbContext.Transacoes
            .Include(t => t.Categoria)
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return this.NotFoundError("Transação não encontrada.");

        transacao.ApplyUpdate(request);
        transacao.Categoria = category;

        var aporte = await _dbContext.ObjetivoAportes.FirstOrDefaultAsync(a => a.TransacaoId == transacao.Id, ct);

        if (request.ObjetivoId.HasValue && transacao.Efetivada)
        {
            if (aporte is null)
            {
                await _dbContext.ObjetivoAportes.AddAsync(BuildAporteFromTransacao(request.ObjetivoId.Value, transacao), ct);
            }
            else
            {
                aporte.ObjetivoId = request.ObjetivoId.Value;
                aporte.Valor = transacao.Valor;
                aporte.Data = transacao.DataLancamento;
                aporte.Observacao = transacao.Observacoes;
            }
        }
        else if (aporte is not null)
        {
            _dbContext.ObjetivoAportes.Remove(aporte);
        }

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
            transacao.AtualizadaEm,
            transacao.ObjetivoId);

    private Task<Category?> GetOwnedCategoryAsync(Guid userId, Guid categoryId, CancellationToken ct)
        => _dbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, ct);

    private async Task<string?> ValidateGoalLinkAsync(Guid userId, TipoTransacoes tipo, Guid? objetivoId, CancellationToken ct)
    {
        if (!objetivoId.HasValue)
            return null;

        if (tipo != TipoTransacoes.Receita)
            return "Somente receitas podem ser vinculadas a um objetivo.";

        var goalExists = await _dbContext.Objetivos
            .AsNoTracking()
            .AnyAsync(o => o.Id == objetivoId.Value && o.UserId == userId, ct);

        return goalExists ? null : "Objetivo informado não pertence ao usuário autenticado.";
    }

    private static ObjetivoAporte BuildAporteFromTransacao(Guid objetivoId, Transacoes transacao)
        => new()
        {
            Id = Guid.NewGuid(),
            ObjetivoId = objetivoId,
            TransacaoId = transacao.Id,
            Valor = transacao.Valor,
            Data = transacao.DataLancamento,
            Observacao = transacao.Observacoes,
            Recorrente = false,
            CriadoEm = DateTime.UtcNow
        };
}
