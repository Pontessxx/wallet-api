namespace AuthApi.Controllers.V2;

[ApiController]
[Controller]
[Route("transfer/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class Transfer : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public Transfer(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Cria uma nova transferência entre carteiras do usuário autenticado.
    /// </summary>
    /// <param name="request">Dados da transferência</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transferência criada</returns>
    /// <response code="201">Transferência criada com sucesso</response>
    /// <response code="400">Dados inválidos da transferência</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpPost("new")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateTransferRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var wallets = await GetUserWalletOriginsAsync(userId, ct);
        if (!wallets.ContainsKey(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        if (request.Valor <= 0)
            return this.BadRequestError("Valor deve ser maior que zero.");

        if (request.Encargos < 0)
            return this.BadRequestError("Encargos não podem ser negativos.");

        if (!wallets.ContainsKey(request.CarteiraDestinoId))
            return this.BadRequestError("Carteira de destino não pertence ao usuário autenticado.");

        if (request.CarteiraId == request.CarteiraDestinoId)
            return this.BadRequestError("Carteira de origem e destino devem ser diferentes.");

        if (!TryResolveExchange(wallets[request.CarteiraId], wallets[request.CarteiraDestinoId], request.Valor + request.Encargos, request.TaxaCambio, out var taxaCambio, out var valorConvertido, out var exchangeError))
            return this.BadRequestError(exchangeError!);

        var transferencia = request.ToEntity();
        transferencia.TaxaCambio = taxaCambio;
        transferencia.ValorConvertido = valorConvertido;

        await _dbContext.TransferenciasCarteira.AddAsync(transferencia, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapTransfer(transferencia));
    }

    /// <summary>
    /// Atualiza uma transferência existente do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transferência</param>
    /// <param name="request">Novos dados da transferência</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transferência atualizada</returns>
    /// <response code="200">Transferência atualizada com sucesso</response>
    /// <response code="400">Dados inválidos da transferência</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transferência não encontrada</response>
    [HttpPut("edit")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTransfer(
        [FromQuery] Guid id,
        [FromBody] UpdateTransferRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var wallets = await GetUserWalletOriginsAsync(userId, ct);
        if (!wallets.ContainsKey(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        if (request.Valor <= 0)
            return this.BadRequestError("Valor deve ser maior que zero.");

        if (request.Encargos < 0)
            return this.BadRequestError("Encargos não podem ser negativos.");

        if (!wallets.ContainsKey(request.CarteiraDestinoId))
            return this.BadRequestError("Carteira de destino não pertence ao usuário autenticado.");

        if (request.CarteiraId == request.CarteiraDestinoId)
            return this.BadRequestError("Carteira de origem e destino devem ser diferentes.");

        if (!TryResolveExchange(wallets[request.CarteiraId], wallets[request.CarteiraDestinoId], request.Valor + request.Encargos, request.TaxaCambio, out var taxaCambio, out var valorConvertido, out var exchangeError))
            return this.BadRequestError(exchangeError!);

        var transferencia = await _dbContext.TransferenciasCarteira
            .FirstOrDefaultAsync(t => t.Id == id && (wallets.ContainsKey(t.CarteiraOrigemId) || wallets.ContainsKey(t.CarteiraDestinoId)), ct);

        if (transferencia is null)
            return this.NotFoundError("Transferência não encontrada.");

        transferencia.ApplyUpdate(request);
        transferencia.TaxaCambio = taxaCambio;
        transferencia.ValorConvertido = valorConvertido;

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapTransfer(transferencia));
    }

    /// <summary>
    /// Remove uma transferência do usuário autenticado.
    /// </summary>
    /// <param name="id">ID da transferência</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Transferência removida com sucesso</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transferência não encontrada</response>
    [HttpDelete("remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransfer([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuário autenticado inválido.");

        var wallets = await GetUserWalletOriginsAsync(userId, ct);

        var transferencia = await _dbContext.TransferenciasCarteira
            .FirstOrDefaultAsync(t => t.Id == id && (wallets.ContainsKey(t.CarteiraOrigemId) || wallets.ContainsKey(t.CarteiraDestinoId)), ct);

        if (transferencia is null)
            return this.NotFoundError("Transferência não encontrada.");

        _dbContext.TransferenciasCarteira.Remove(transferencia);
        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }

    private async Task<Dictionary<Guid, WalletOrigin>> GetUserWalletOriginsAsync(Guid userId, CancellationToken ct)
    {
        return await _dbContext.Carteiras
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .ToDictionaryAsync(c => c.Id, c => c.Origem, ct);
    }

    private static bool TryResolveExchange(
        WalletOrigin origem,
        WalletOrigin destino,
        decimal valorTotal,
        decimal? taxaCambioInformada,
        out decimal? taxaCambio,
        out decimal? valorConvertido,
        out string? error)
    {
        if (origem == destino)
        {
            taxaCambio = null;
            valorConvertido = null;
            error = null;
            return true;
        }

        if (taxaCambioInformada is not > 0)
        {
            taxaCambio = null;
            valorConvertido = null;
            error = "Informe a cotação para transferência entre moedas diferentes.";
            return false;
        }

        taxaCambio = taxaCambioInformada;
        valorConvertido = origem == WalletOrigin.Nacional
            ? Math.Round(valorTotal / taxaCambioInformada.Value, 2)
            : Math.Round(valorTotal * taxaCambioInformada.Value, 2);
        error = null;
        return true;
    }

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
            null,
            transferencia.TaxaCambio,
            transferencia.ValorConvertido);
}
