namespace AuthApi.Controllers.V2;

[ApiController]
[Route("transaction/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class TransactionController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionController(ApplicationDbContext dbContext)
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

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        if (request.Valor <= 0)
            return this.BadRequestError("Valor deve ser maior que zero.");

        if (request.Encargos < 0)
            return this.BadRequestError("Encargos não podem ser negativos.");

        if (!walletIds.Contains(request.CarteiraDestinoId))
            return this.BadRequestError("Carteira de destino não pertence ao usuário autenticado.");

        if (request.CarteiraId == request.CarteiraDestinoId)
            return this.BadRequestError("Carteira de origem e destino devem ser diferentes.");

        var transferencia = request.ToEntity();

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

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return this.BadRequestError("Carteira informada não pertence ao usuário autenticado.");

        if (request.Valor <= 0)
            return this.BadRequestError("Valor deve ser maior que zero.");

        if (request.Encargos < 0)
            return this.BadRequestError("Encargos não podem ser negativos.");

        if (!walletIds.Contains(request.CarteiraDestinoId))
            return this.BadRequestError("Carteira de destino não pertence ao usuário autenticado.");

        if (request.CarteiraId == request.CarteiraDestinoId)
            return this.BadRequestError("Carteira de origem e destino devem ser diferentes.");

        var transferencia = await _dbContext.TransferenciasCarteira
            .FirstOrDefaultAsync(t => t.Id == id && (walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId)), ct);

        if (transferencia is null)
            return this.NotFoundError("Transferência não encontrada.");

        transferencia.ApplyUpdate(request);

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapTransfer(transferencia));
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
            transferencia.AtualizadaEm);
}
