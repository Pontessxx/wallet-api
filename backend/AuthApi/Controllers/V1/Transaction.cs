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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacao = await _dbContext.Transacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is not null)
            return Ok(MapTransaction(transacao));

        var transferencia = await _dbContext.TransferenciasCarteira
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && (walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId)), ct);

        if (transferencia is null)
            return NotFound(new { message = "Transação não encontrada." });

        return Ok(MapTransfer(transferencia));
    }

    [HttpPost("transfer")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        [FromHeader(Name = "X-TransactionType")] TipoTransacoes transactionType,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!IsValidTransactionTypeHeader(transactionType))
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacoes>());
            return BadRequest(new { message = $"Header X-TransactionType inválido. Valores permitidos: {allowedValues}." });
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        if (request.Valor <= 0)
            return BadRequest(new { message = "Valor deve ser maior que zero." });

        if (request.Encargos < 0)
            return BadRequest(new { message = "Encargos não podem ser negativos." });

        if (transactionType == TipoTransacoes.Transferencia)
        {
            if (!request.CarteiraDestinoId.HasValue)
                return BadRequest(new { message = "CarteiraDestinoId é obrigatório para transferências." });

            if (!walletIds.Contains(request.CarteiraDestinoId.Value))
                return BadRequest(new { message = "Carteira de destino não pertence ao usuário autenticado." });

            if (request.CarteiraId == request.CarteiraDestinoId.Value)
                return BadRequest(new { message = "Carteira de origem e destino devem ser diferentes." });

            var transferencia = new TransferenciaCarteira
            {
                Id = Guid.NewGuid(),
                CarteiraOrigemId = request.CarteiraId,
                CarteiraDestinoId = request.CarteiraDestinoId.Value,
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

        var transacao = new Transacoes
        {
            Id = Guid.NewGuid(),
            CarteiraId = request.CarteiraId,
            Tipo = transactionType.ToString(),
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

        await _dbContext.Transacoes.AddAsync(transacao, ct);
        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapTransaction(transacao));
    }

    [HttpPut("transfer/{id:guid}")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        [FromHeader(Name = "X-TransactionType")] TipoTransacoes transactionType,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!IsValidTransactionTypeHeader(transactionType))
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacoes>());
            return BadRequest(new { message = $"Header X-TransactionType inválido. Valores permitidos: {allowedValues}." });
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        if (request.Valor <= 0)
            return BadRequest(new { message = "Valor deve ser maior que zero." });

        if (request.Encargos < 0)
            return BadRequest(new { message = "Encargos não podem ser negativos." });

        if (transactionType == TipoTransacoes.Transferencia)
        {
            if (!request.CarteiraDestinoId.HasValue)
                return BadRequest(new { message = "CarteiraDestinoId é obrigatório para transferências." });

            if (!walletIds.Contains(request.CarteiraDestinoId.Value))
                return BadRequest(new { message = "Carteira de destino não pertence ao usuário autenticado." });

            if (request.CarteiraId == request.CarteiraDestinoId.Value)
                return BadRequest(new { message = "Carteira de origem e destino devem ser diferentes." });

            var transferencia = await _dbContext.TransferenciasCarteira
                .FirstOrDefaultAsync(t => t.Id == id && (walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId)), ct);

            if (transferencia is null)
                return NotFound(new { message = "Transferência não encontrada." });

            transferencia.CarteiraOrigemId = request.CarteiraId;
            transferencia.CarteiraDestinoId = request.CarteiraDestinoId.Value;
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

        var transacao = await _dbContext.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return NotFound(new { message = "Transação não encontrada." });

        transacao.CarteiraId = request.CarteiraId;
        transacao.Tipo = transactionType.ToString();
        transacao.Valor = request.Valor;
        transacao.Encargos = request.Encargos;
        transacao.ValorTotal = request.Valor + request.Encargos;
        transacao.Efetivada = request.Efetivada;
        transacao.DataLancamento = request.DataLancamento;
        transacao.DataVencimento = request.DataVencimento;
        transacao.DataEfetivacao = request.Efetivada ? request.DataEfetivacao ?? DateTime.UtcNow : null;
        transacao.Observacoes = request.Observacoes;
        transacao.AtualizadaEm = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapTransaction(transacao));
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(TransactionHistoryResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromHeader(Name = "X-TipoTransacoes")] TipoTransacoes? transactionTypeFilter,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacoes>());
            return BadRequest(new { message = $"Header X-TipoTransacoes inválido. Valores permitidos: {allowedValues}." });
        }

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacoesQuery = _dbContext.Transacoes
            .AsNoTracking()
            .Where(t => walletIds.Contains(t.CarteiraId));

        if (transactionTypeFilter.HasValue)
            transacoesQuery = transacoesQuery.Where(t => t.Tipo == transactionTypeFilter.Value.ToString());

        var transacoes = await transacoesQuery.ToListAsync(ct);

        var transferencias = new List<TransferenciaCarteira>();
        if (!transactionTypeFilter.HasValue || transactionTypeFilter.Value == TipoTransacoes.Transferencia)
        {
            transferencias = await _dbContext.TransferenciasCarteira
                .AsNoTracking()
                .Where(t => walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId))
                .ToListAsync(ct);
        }

        var historico = transacoes
            .Select(MapTransaction)
            .Concat(transferencias.Select(MapTransfer))
            .OrderByDescending(t => t.DataLancamento)
            .ToList();

        return Ok(new TransactionHistoryResult(historico));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);

        var transacao = await _dbContext.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is not null)
        {
            _dbContext.Transacoes.Remove(transacao);
            await _dbContext.SaveChangesAsync(ct);
            return NoContent();
        }

        var transferencia = await _dbContext.TransferenciasCarteira
            .FirstOrDefaultAsync(t => t.Id == id && (walletIds.Contains(t.CarteiraOrigemId) || walletIds.Contains(t.CarteiraDestinoId)), ct);

        if (transferencia is null)
            return NotFound(new { message = "Transação não encontrada." });

        _dbContext.TransferenciasCarteira.Remove(transferencia);
        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
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
        [FromHeader(Name = "X-TipoTransacaoBolsa")] TipoTransacaoBolsa transactionType,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!IsValidExchangeTypeHeader(transactionType))
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return BadRequest(new { message = $"Header X-TipoTransacaoBolsa inválido. Valores permitidos: {allowedValues}." });
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
            Lado = transactionType,
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

    [HttpPut("exchange/{id:guid}")]
    [ProducesResponseType(typeof(ExchangeTransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExchange(
        Guid id,
        [FromBody] UpdateExchangeRequest request,
        [FromHeader(Name = "X-TipoTransacaoBolsa")] TipoTransacaoBolsa transactionType,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!IsValidExchangeTypeHeader(transactionType))
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return BadRequest(new { message = $"Header X-TipoTransacaoBolsa inválido. Valores permitidos: {allowedValues}." });
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
        exchange.Lado = transactionType;
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
        [FromHeader(Name = "X-TipoTransacaoBolsa")] TipoTransacaoBolsa? exchangeTypeFilter,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacaoBolsa>());
            return BadRequest(new { message = $"Header X-TipoTransacaoBolsa inválido. Valores permitidos: {allowedValues}." });
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

    [HttpDelete("exchange/{id:guid}")]
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

    private bool IsValidTransactionTypeHeader(TipoTransacoes transactionType)
    {
        if (!Request.Headers.TryGetValue("X-TransactionType", out var rawHeaderValue))
            return false;

        if (!Enum.TryParse<TipoTransacoes>(rawHeaderValue.ToString(), true, out var parsedHeaderValue))
            return false;

        return transactionType == parsedHeaderValue;
    }

    private bool IsValidExchangeTypeHeader(TipoTransacaoBolsa transactionType)
    {
        if (!Request.Headers.TryGetValue("X-TipoTransacaoBolsa", out var rawHeaderValue))
            return false;

        if (!Enum.TryParse<TipoTransacaoBolsa>(rawHeaderValue.ToString(), true, out var parsedHeaderValue))
            return false;

        return transactionType == parsedHeaderValue;
    }

    private static TipoTransacoes ParseTransactionType(string type)
    {
        if (Enum.TryParse<TipoTransacoes>(type, true, out var parsedType))
            return parsedType;

        return TipoTransacoes.Despesa;
    }

    private static TransactionResult MapTransaction(Transacoes transacao)
        => new(
            transacao.Id,
            transacao.CarteiraId,
            null,
            ParseTransactionType(transacao.Tipo),
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

    private static TransactionResult MapTransfer(TransferenciaCarteira transferencia)
        => new(
            transferencia.Id,
            transferencia.CarteiraOrigemId,
            transferencia.CarteiraDestinoId,
            TipoTransacoes.Transferencia,
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