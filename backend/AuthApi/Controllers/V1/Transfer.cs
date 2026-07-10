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

    [HttpGet("list/{id:guid}")]
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

        if (transacao is null)
            return NotFound(new { message = "Transação não encontrada." });

        return Ok(MapTransaction(transacao));
    }

    [HttpPost("new")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEntry(
        [FromBody] CreateEntryRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        var transacao = new Transacoes
        {
            Id = Guid.NewGuid(),
            CarteiraId = request.CarteiraId,
            Tipo = request.Tipo,
            Categoria = request.Categoria,
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

    [HttpPut("edit/{id:guid}")]
    [ProducesResponseType(typeof(TransactionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntry(
        Guid id,
        [FromBody] UpdateEntryRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        var walletIds = await GetUserWalletIdsAsync(userId, ct);
        if (!walletIds.Contains(request.CarteiraId))
            return BadRequest(new { message = "Carteira informada não pertence ao usuário autenticado." });

        var transacao = await _dbContext.Transacoes
            .FirstOrDefaultAsync(t => t.Id == id && walletIds.Contains(t.CarteiraId), ct);

        if (transacao is null)
            return NotFound(new { message = "Transação não encontrada." });

        transacao.CarteiraId = request.CarteiraId;
        transacao.Tipo = request.Tipo;
        transacao.Categoria = request.Categoria;
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
        [FromQuery(Name = "tipo")] TipoTransacoes? transactionTypeFilter,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized(new { message = "Usuário autenticado inválido." });

        if (!ModelState.IsValid)
        {
            var allowedValues = string.Join(", ", Enum.GetNames<TipoTransacoes>());
            return BadRequest(new { message = $"Parâmetro de query tipo inválido. Valores permitidos: {allowedValues}." });
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

    [HttpDelete("remove/{id:guid}")]
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

        if (transacao is null)
            return NotFound(new { message = "Transação não encontrada." });

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
