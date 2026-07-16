namespace AuthApi.Controllers.V2;

[ApiController]
[Route("wallet/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class WalletController : ControllerBase
{
    private readonly ContaCarteiraService _contaCarteiraService;

    public WalletController(ContaCarteiraService contaCarteiraService)
    {
        _contaCarteiraService = contaCarteiraService;
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
    /// <param name="ct">Cancellation token</param>
    /// <returns>Resumo de saldos das carteiras</returns>
    /// <response code="200">Resumo retornado com sucesso</response>
    /// <response code="400">Parâmetro de categoria inválido</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(WalletSummaryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary([FromQuery] WalletCategory? categoria, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<WalletCategory>());
            return this.BadRequestError($"Parâmetro de query categoria inválido. Valores permitidos: {allowedValues}.");
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
