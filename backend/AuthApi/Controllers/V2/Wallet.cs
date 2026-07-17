namespace AuthApi.Controllers.V2;

[ApiController]
[Route("wallet/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class WalletController : ControllerBase
{
    private readonly ContaCarteiraService _contaCarteiraService;
    private readonly ApplicationDbContext _dbContext;

    public WalletController(ContaCarteiraService contaCarteiraService, ApplicationDbContext dbContext)
    {
        _contaCarteiraService = contaCarteiraService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Cria uma nova conta/carteira para o usuário autenticado.
    /// </summary>
    /// <param name="request">Dados da carteira</param>
    /// <param name="walletType">Tipo da carteira enviado no header X-WalletType</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Carteira criada</returns>
    /// <response code="201">Carteira criada com sucesso</response>
    /// <response code="400">Header ou dados inválidos</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpPost("accounts/create")]
    [ProducesResponseType(typeof(CarteiraResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCarteiraRequest request,
        [FromHeader(Name = "X-WalletType")] WalletCategory walletType,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!IsValidWalletTypeHeader(walletType))
        {
            var allowedValues = string.Join(", ", Enum.GetNames<WalletCategory>());
            return this.BadRequestError($"Header X-WalletType inválido. Valores permitidos: {allowedValues}.");
        }

        try
        {
            var carteira = await _contaCarteiraService.CreateAsync(
                userId,
                request.Nome,
                walletType,
                request.SaldoInicial,
                ct);

            return StatusCode(StatusCodes.Status201Created, carteira);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestError(ex.Message);
        }
    }

    /// <summary>
    /// Atualiza uma carteira existente do usuário autenticado.
    /// </summary>
    /// <param name="request">Dados para atualização da carteira</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Carteira atualizada</returns>
    /// <response code="200">Carteira atualizada com sucesso</response>
    /// <response code="400">Dados inválidos para atualização</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Carteira não encontrada</response>
    [HttpPut("accounts/edit")]
    [ProducesResponseType(typeof(CarteiraResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Edit([FromBody] EditCarteiraRequest request, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        try
        {
            var carteira = await _contaCarteiraService.UpdateAsync(userId, request.Id, request.Nome, request.Categoria, ct);
            return Ok(carteira);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase))
        {
            return this.NotFoundError(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return this.BadRequestError(ex.Message);
        }
    }

    /// <summary>
    /// Remove uma carteira existente do usuário autenticado.
    /// </summary>
    /// <param name="request">ID da carteira a ser removida</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Carteira removida com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Carteira não encontrada</response>
    [HttpDelete("accounts/remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete([FromBody] RemoveCarteiraRequest request, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        try
        {
            await _contaCarteiraService.DeleteAsync(userId, request.Id, ct);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return this.NotFoundError(ex.Message);
        }
    }

    /// <summary>
    /// Retorna as contas/carteiras do usuário autenticado, com filtro opcional por categoria.
    /// </summary>
    /// <param name="categoria">Categoria opcional para filtrar as carteiras</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista de contas/carteiras</returns>
    /// <response code="200">Contas retornadas com sucesso</response>
    /// <response code="400">Parâmetro de categoria inválido</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(WalletAccountsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAccounts([FromQuery] WalletCategory? categoria, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<WalletCategory>());
            return this.BadRequestError($"Parâmetro de query categoria inválido. Valores permitidos: {allowedValues}.");
        }

        var accounts = await _contaCarteiraService.GetAllAsync(userId, ct);

        if (!categoria.HasValue)
            return Ok(accounts);

        var filtered = accounts.Carteiras
            .Where(c => c.Categoria == categoria.Value)
            .ToList();

        return Ok(new WalletAccountsResult(filtered));
    }

    /// <summary>
    /// Retorna o resumo de saldos das carteiras do usuário autenticado, com filtro opcional por categoria.
    /// </summary>
    /// <param name="categoria">Categoria opcional para filtrar o resumo</param>
    /// <param name="periodType">Filtro opcional de período: range, monthly, yearly</param>
    /// <param name="startDate">Data inicial (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="endDate">Data final (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="year">Ano quando periodType=monthly/yearly</param>
    /// <param name="month">Mês 1..12 quando periodType=monthly</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Resumo de saldos das carteiras</returns>
    /// <response code="200">Resumo retornado com sucesso</response>
    /// <response code="400">Parâmetro de categoria/período inválido</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(WalletSummaryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] WalletCategory? categoria,
        [FromQuery] string? periodType,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<WalletCategory>());
            return this.BadRequestError($"Parâmetro de query categoria inválido. Valores permitidos: {allowedValues}.");
        }

        var hasPeriodFilter = !string.IsNullOrWhiteSpace(periodType)
            || !string.IsNullOrWhiteSpace(startDate)
            || !string.IsNullOrWhiteSpace(endDate)
            || year.HasValue
            || month.HasValue;

        if (!PeriodQueryParser.TryResolveDateRange(
                periodType,
                startDate,
                endDate,
                year,
                month,
                requirePeriodType: hasPeriodFilter,
                out var rangeStart,
                out var rangeEndExclusive,
                out var periodError))
        {
            return this.BadRequestError(periodError!);
        }

        if (hasPeriodFilter)
        {
            var periodSummary = await BuildPeriodSummaryAsync(userId, categoria, rangeStart, rangeEndExclusive, ct);
            return Ok(periodSummary);
        }

        var summary = await _contaCarteiraService.GetSummaryAsync(userId, ct);

        if (!categoria.HasValue)
            return Ok(summary);

        var filtered = summary.Carteiras
            .Where(c => c.Categoria == categoria.Value)
            .ToList();

        var filteredTotal = filtered.Sum(c => c.Saldo);
        return Ok(new WalletSummaryResult(filtered, filteredTotal));
    }

    private async Task<WalletSummaryResult> BuildPeriodSummaryAsync(
        Guid userId,
        WalletCategory? categoria,
        DateTime rangeStart,
        DateTime rangeEndExclusive,
        CancellationToken ct)
    {
        var carteirasQuery = _dbContext.Carteiras
            .AsNoTracking()
            .Where(c => c.UserId == userId);

        if (categoria.HasValue)
            carteirasQuery = carteirasQuery.Where(c => c.Categoria == categoria.Value);

        var carteiras = await carteirasQuery.ToListAsync(ct);

        if (carteiras.Count == 0)
            return new WalletSummaryResult([], 0m);

        var walletIds = carteiras.Select(c => c.Id).ToList();

        var transacaoTotals = await _dbContext.Transacoes
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraId))
            .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive)
            .GroupBy(t => t.CarteiraId)
            .Select(g => new
            {
                CarteiraId = g.Key,
                Receitas = g.Where(x => x.Tipo == TipoTransacoes.Receita).Sum(x => x.ValorTotal),
                Despesas = g.Where(x => x.Tipo == TipoTransacoes.Despesa).Sum(x => x.ValorTotal),
            })
            .ToListAsync(ct);

        var transferInTotals = await _dbContext.TransferenciasCarteira
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraDestinoId))
            .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive)
            .GroupBy(t => t.CarteiraDestinoId)
            .Select(g => new
            {
                CarteiraId = g.Key,
                Total = g.Sum(x => x.ValorTotal),
            })
            .ToListAsync(ct);

        var transferOutTotals = await _dbContext.TransferenciasCarteira
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraOrigemId))
            .Where(t => t.DataLancamento >= rangeStart && t.DataLancamento < rangeEndExclusive)
            .GroupBy(t => t.CarteiraOrigemId)
            .Select(g => new
            {
                CarteiraId = g.Key,
                Total = g.Sum(x => x.ValorTotal),
            })
            .ToListAsync(ct);

        var transacaoByWallet = transacaoTotals.ToDictionary(x => x.CarteiraId);
        var transferInByWallet = transferInTotals.ToDictionary(x => x.CarteiraId, x => x.Total);
        var transferOutByWallet = transferOutTotals.ToDictionary(x => x.CarteiraId, x => x.Total);

        var carteiraResults = carteiras
            .Select(carteira =>
            {
                var receitas = transacaoByWallet.TryGetValue(carteira.Id, out var totals) ? totals.Receitas : 0m;
                var despesas = transacaoByWallet.TryGetValue(carteira.Id, out totals) ? totals.Despesas : 0m;
                var transferenciaEntrada = transferInByWallet.TryGetValue(carteira.Id, out var inTotal) ? inTotal : 0m;
                var transferenciaSaida = transferOutByWallet.TryGetValue(carteira.Id, out var outTotal) ? outTotal : 0m;
                var transferencias = transferenciaEntrada - transferenciaSaida;
                var saldo = carteira.SaldoInicial + receitas - despesas + transferencias;

                return new CarteiraResult(
                    carteira.Id,
                    carteira.Nome,
                    carteira.Categoria,
                    carteira.SaldoInicial,
                    receitas,
                    despesas,
                    transferencias,
                    saldo,
                    carteira.SaldoProjetado);
            })
            .ToList();

        var saldoTotal = carteiraResults.Sum(c => c.Saldo);
        return new WalletSummaryResult(carteiraResults, saldoTotal);
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }

    private bool IsValidWalletTypeHeader(WalletCategory walletType)
    {
        if (!Request.Headers.TryGetValue("X-WalletType", out var rawHeaderValue))
            return false;

        if (!Enum.TryParse<WalletCategory>(rawHeaderValue.ToString(), true, out var parsedHeaderValue))
            return false;

        return walletType == parsedHeaderValue;
    }
}
