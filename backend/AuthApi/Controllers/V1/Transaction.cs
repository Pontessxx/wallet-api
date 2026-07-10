namespace AuthApi.Controllers;

[ApiController]
[Route("transaction/v1")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
public class TransactionController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public TransactionController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Cria uma transferência entre carteiras do usuário autenticado.
    /// </summary>
    /// <param name="request">Dados da transferência (origem, destino e valores)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transferência criada</returns>
    /// <response code="201">Transferência criada com sucesso</response>
    /// <response code="400">Dados inválidos para transferência</response>
    /// <response code="401">Usuário autenticado inválido</response>
    [HttpPost("new")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateTransferRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        if (request.Valor <= 0)
            return BadRequest(new { message = "Valor deve ser maior que zero." });

        if (request.Encargos < 0)
            return BadRequest(new { message = "Encargos não podem ser negativos." });

        if (!walletIds.Contains(request.CarteiraDestinoId))
            return BadRequest(new { message = "Carteira de destino não pertence ao usuário autenticado." });

        if (request.CarteiraId == request.CarteiraDestinoId)
            return BadRequest(new { message = "Carteira de origem e destino devem ser diferentes." });

        var transferencia = new TransferenciaCarteira
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

        await _dbContext.TransferenciasCarteira.AddAsync(transferencia, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapTransfer(transferencia));
    }

    /// <summary>
    /// Atualiza uma transferência existente.
    /// </summary>
    /// <param name="id">ID da transferência</param>
    /// <param name="request">Novos dados da transferência</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Transferência atualizada</returns>
    /// <response code="200">Transferência atualizada com sucesso</response>
    /// <response code="400">Dados inválidos para transferência</response>
    /// <response code="401">Usuário autenticado inválido</response>
    /// <response code="404">Transferência não encontrada</response>
    [HttpPut("edit/{id:guid}")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTransfer(
        Guid id,
        [FromBody] UpdateTransferRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        if (request.Valor <= 0)
            return BadRequest(new { message = "Valor deve ser maior que zero." });

        if (request.Encargos < 0)
            return BadRequest(new { message = "Encargos não podem ser negativos." });

        if (!walletIds.Contains(request.CarteiraDestinoId))
            return BadRequest(new { message = "Carteira de destino não pertence ao usuário autenticado." });

        if (request.CarteiraId == request.CarteiraDestinoId)
            return BadRequest(new { message = "Carteira de origem e destino devem ser diferentes." });

        var transferencia = await _dbContext.TransferenciasCarteira
            .FirstOrDefaultAsync(t => t.Id == id && (walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId)), ct);

        if (transferencia is null)
            return NotFound(new { message = "Transferência não encontrada." });

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

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapTransfer(transferencia));
    }

    [HttpGet("exchange/{id:guid}")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExchangeById(Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var exchange = await _dbContext.TransacoesBolsa
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (exchange is null)
            return NotFound(new { message = "Transação de bolsa não encontrada." });

        return Ok(MapExchange(exchange));
    }

    [HttpPost("exchange")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateExchange(
        [FromBody] CreateExchangeRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!request.Lado.HasValue)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return BadRequest(new { message = $"Campo lado é obrigatório. Valores permitidos: {allowedValues}." });
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        if (request.Quantidade <= 0)
            return BadRequest(new { message = "Quantidade deve ser maior que zero." });

        if (request.PrecoUnitario <= 0)
            return BadRequest(new { message = "Preço unitário deve ser maior que zero." });

        if (request.Encargos < 0)
            return BadRequest(new { message = "Encargos não podem ser negativos." });

        var valor = request.Quantidade * request.PrecoUnitario;
        var exchange = new TransacaoBolsa
        {
            Id = Guid.NewGuid(),
            CarteiraId = request.CarteiraId,
            CodigoAtivo = request.CodigoAtivo.Trim().ToUpperInvariant(),
            Lado = request.Lado.Value,
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

        await _dbContext.TransacoesBolsa.AddAsync(exchange, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapExchange(exchange));
    }

    [HttpPut("exchange/edit/{id:guid}")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExchange(
        Guid id,
        [FromBody] UpdateExchangeRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!request.Lado.HasValue)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return BadRequest(new { message = $"Campo lado é obrigatório. Valores permitidos: {allowedValues}." });
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        if (request.Quantidade <= 0)
            return BadRequest(new { message = "Quantidade deve ser maior que zero." });

        if (request.PrecoUnitario <= 0)
            return BadRequest(new { message = "Preço unitário deve ser maior que zero." });

        if (request.Encargos < 0)
            return BadRequest(new { message = "Encargos não podem ser negativos." });

        var exchange = await _dbContext.TransacoesBolsa
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (exchange is null)
            return NotFound(new { message = "Transação de bolsa não encontrada." });

        var valor = request.Quantidade * request.PrecoUnitario;

        exchange.CarteiraId = request.CarteiraId;
        exchange.CodigoAtivo = request.CodigoAtivo.Trim().ToUpperInvariant();
        exchange.Lado = request.Lado.Value;
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

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapExchange(exchange));
    }

    [HttpGet("exchange/history")]
    [ProducesResponseType(typeof(ExchangeHistoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetExchangeHistory(
        [FromQuery(Name = "lado")] TipoTransacaoBolsa? exchangeTypeFilter,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return BadRequest(new { message = $"Parâmetro de query lado inválido. Valores permitidos: {allowedValues}." });
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var exchangeQuery = _dbContext.TransacoesBolsa
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraId));

        if (exchangeTypeFilter.HasValue)
            exchangeQuery = exchangeQuery.Where(t => t.Lado == exchangeTypeFilter.Value);

        var historico = await exchangeQuery
            .OrderByDescending(t => t.DataLancamento)
            .Select(t => MapExchange(t))
            .ToListAsync(ct);

        return Ok(new ExchangeHistoryResult(historico));
    }

    [HttpDelete("exchange/remove/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExchange(Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        var exchange = await _dbContext.TransacoesBolsa
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (exchange is null)
            return NotFound(new { message = "Transação de bolsa não encontrada." });

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