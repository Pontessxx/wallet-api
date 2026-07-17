namespace AuthApi.Controllers.V2;

[ApiController]
[Route("exchange/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class ExchangeController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public ExchangeController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Cria uma nova transação de bolsa para o usuário autenticado.
    /// </summary>
    /// <param name="request">Dados da transação de bolsa</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação de bolsa criada</returns>
    /// <response code="201">Transação de bolsa criada com sucesso</response>
    /// <response code="400">Dados inválidos da transação</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpPost("new")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExchangeRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!request.Lado.HasValue)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return this.BadRequestError($"Campo lado é obrigatório. Valores permitidos: {allowedValues}.");
        }

        var walletIds = await GetUserInvestmentWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada deve ser do tipo Investimento e pertencer ao usuário autenticado.");

        if (request.Quantidade <= 0)
            return this.BadRequestError("Quantidade deve ser maior que zero.");

        if (request.PrecoUnitario <= 0)
            return this.BadRequestError("Preço unitário deve ser maior que zero.");

        if (request.Encargos < 0)
            return this.BadRequestError("Encargos não podem ser negativos.");

        var exchange = request.ToEntity();

        await _dbContext.TransacoesBolsa.AddAsync(exchange, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapExchange(exchange));
    }

    /// <summary>
    /// Atualiza uma transação de bolsa existente do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transação de bolsa</param>
    /// <param name="request">Novos dados da transação de bolsa</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação de bolsa atualizada</returns>
    /// <response code="200">Transação de bolsa atualizada com sucesso</response>
    /// <response code="400">Dados inválidos da transação</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação de bolsa não encontrada</response>
    [HttpPut("edit")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromQuery] Guid id,
        [FromBody] UpdateExchangeRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!request.Lado.HasValue)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return this.BadRequestError($"Campo lado é obrigatório. Valores permitidos: {allowedValues}.");
        }

        var walletIds = await GetUserInvestmentWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada deve ser do tipo Investimento e pertencer ao usuário autenticado.");

        if (request.Quantidade <= 0)
            return this.BadRequestError("Quantidade deve ser maior que zero.");

        if (request.PrecoUnitario <= 0)
            return this.BadRequestError("Preço unitário deve ser maior que zero.");

        if (request.Encargos < 0)
            return this.BadRequestError("Encargos não podem ser negativos.");

        var exchange = await _dbContext.TransacoesBolsa
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (exchange is null)
            return this.NotFoundError("Transação de bolsa não encontrada.");

        exchange.ApplyUpdate(request);

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapExchange(exchange));
    }

    /// <summary>
    /// Retorna uma transação de bolsa específica do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transação de bolsa</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transação de bolsa encontrada</returns>
    /// <response code="200">Transação de bolsa encontrada com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação de bolsa não encontrada</response>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserInvestmentWalletIdsAsync(userId, ct);

        var exchange = await _dbContext.TransacoesBolsa
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (exchange is null)
            return this.NotFoundError("Transação de bolsa não encontrada.");

        return Ok(MapExchange(exchange));
    }

    /// <summary>
    /// Remove uma transação de bolsa do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transação de bolsa</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Transação de bolsa removida com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transação de bolsa não encontrada</response>
    [HttpDelete("remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var walletIds = await GetUserInvestmentWalletIdsAsync(userId, ct);
        var exchange = await _dbContext.TransacoesBolsa
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (exchange is null)
            return this.NotFoundError("Transação de bolsa não encontrada.");

        _dbContext.TransacoesBolsa.Remove(exchange);
        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
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
