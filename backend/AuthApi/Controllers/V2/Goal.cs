namespace AuthApi.Controllers.V2;

[ApiController]
[Route("goal/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class GoalController : ControllerBase
{
    private const string DefaultIconKey = "target";
    private static readonly HashSet<string> AllowedIconKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "target",
        "plane",
        "graduation-cap",
        "footprints",
        "watch",
        "home",
        "car",
        "gift",
        "piggy-bank",
        "heart",
        "laptop",
        "smartphone",
        "camera",
        "book-open",
        "briefcase",
        "dumbbell",
        "gamepad-2",
        "umbrella",
        "star",
        "wallet"
    };

    private readonly ApplicationDbContext _dbContext;

    public GoalController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lista objetivos do usuario autenticado com filtros opcionais via query.
    /// </summary>
    /// <param name="id">Filtro opcional por id do objetivo</param>
    /// <param name="carteiraId">Filtro opcional por id da carteira</param>
    /// <param name="periodType">Filtro opcional de período: range, monthly, yearly</param>
    /// <param name="startDate">Data inicial (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="endDate">Data final (YYYY-MM-DD) quando periodType=range</param>
    /// <param name="year">Ano quando periodType=monthly/yearly</param>
    /// <param name="month">Mês 1..12 quando periodType=monthly</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista de objetivos do usuario</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(V2GoalListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? id,
        [FromQuery] Guid? carteiraId,
        [FromQuery] string? periodType,
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        if (!ModelState.IsValid)
            return this.BadRequestError("Parametros de query invalidos.");

        if (carteiraId.HasValue && !await WalletBelongsToUserAsync(userId, carteiraId.Value, ct))
            return this.BadRequestError("Carteira informada nao pertence ao usuario autenticado.");

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

        var query = _dbContext.Objetivos
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        if (id.HasValue)
            query = query.Where(o => o.Id == id.Value);

        if (carteiraId.HasValue)
            query = query.Where(o => o.CarteiraId == carteiraId.Value);

        if (hasPeriodFilter)
            query = query.Where(o => o.CriadaEm >= rangeStart && o.CriadaEm < rangeEndExclusive);

        var objetivoEntities = await query
            .Include(o => o.Carteira)
            .Include(o => o.Aportes)
            .OrderByDescending(o => o.CriadaEm)
            .ToListAsync(ct);

        var objetivos = objetivoEntities
            .Select(MapGoal)
            .ToList();

        return Ok(new V2GoalListResult(objetivos));
    }

    /// <summary>
    /// Cria um novo objetivo para o usuario autenticado.
    /// </summary>
    /// <param name="carteiraId">Id opcional da carteira enviado por query</param>
    /// <param name="request">Dados do objetivo</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Objetivo criado</returns>
    [HttpPost("new")]
    [ProducesResponseType(typeof(V2GoalResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromQuery] Guid? carteiraId,
        [FromBody] V2CreateGoalRequest request,
        CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        var validationError = await ValidateGoalRequestAsync(userId, request.Nome, request.ValorTotal, request.Meses, carteiraId, request.IconKey, ct);
        if (validationError is not null)
            return this.BadRequestError(validationError);

        var goal = new Objetivo
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CarteiraId = carteiraId,
            Nome = request.Nome.Trim(),
            IconKey = NormalizeIconKey(request.IconKey),
            ValorTotal = request.ValorTotal,
            Meses = request.Meses,
            ValorMensal = ComputeMonthlyAmount(request.ValorTotal, request.Meses),
            CriadaEm = DateTime.UtcNow
        };

        await _dbContext.Objetivos.AddAsync(goal, ct);
        await _dbContext.SaveChangesAsync(ct);

        if (carteiraId.HasValue)
        {
            goal.Carteira = await _dbContext.Carteiras
                .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == carteiraId.Value, ct);
        }

        return StatusCode(StatusCodes.Status201Created, MapGoal(goal));
    }

    /// <summary>
    /// Atualiza um objetivo existente do usuario autenticado.
    /// </summary>
    /// <param name="id">Id do objetivo</param>
    /// <param name="request">Novos dados</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Objetivo atualizado</returns>
    [HttpPut("edit")]
    [ProducesResponseType(typeof(V2GoalResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromQuery] Guid id, [FromBody] V2UpdateGoalRequest request, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        var validationError = await ValidateGoalRequestAsync(userId, request.Nome, request.ValorTotal, request.Meses, request.CarteiraId, request.IconKey, ct);
        if (validationError is not null)
            return this.BadRequestError(validationError);

        var goal = await _dbContext.Objetivos
            .Include(o => o.Carteira)
            .Include(o => o.Aportes)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

        if (goal is null)
            return this.NotFoundError("Objetivo nao encontrado.");

        goal.Nome = request.Nome.Trim();
        goal.IconKey = NormalizeIconKey(request.IconKey);
        goal.ValorTotal = request.ValorTotal;
        goal.Meses = request.Meses;
        goal.CarteiraId = request.CarteiraId;
        goal.ValorMensal = ComputeMonthlyAmount(request.ValorTotal, request.Meses);
        goal.AtualizadaEm = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);
        return Ok(MapGoal(goal));
    }

    /// <summary>
    /// Remove um objetivo do usuario autenticado.
    /// </summary>
    /// <param name="id">Id do objetivo</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteudo</returns>
    [HttpDelete("remove")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        var goal = await _dbContext.Objetivos
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

        if (goal is null)
            return this.NotFoundError("Objetivo nao encontrado.");

        _dbContext.Objetivos.Remove(goal);
        await _dbContext.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Registra um deposito (aporte) avulso em um objetivo.
    /// </summary>
    /// <param name="id">Id do objetivo</param>
    /// <param name="request">Dados do deposito</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Objetivo atualizado</returns>
    [HttpPost("aporte/new")]
    [ProducesResponseType(typeof(V2GoalResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAporte([FromQuery] Guid id, [FromBody] V2CreateGoalAporteRequest request, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        if (request.Valor <= 0)
            return this.BadRequestError("Valor do deposito deve ser maior que zero.");

        var goal = await _dbContext.Objetivos
            .Include(o => o.Carteira)
            .Include(o => o.Aportes)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

        if (goal is null)
            return this.NotFoundError("Objetivo nao encontrado.");

        var aporte = new ObjetivoAporte
        {
            Id = Guid.NewGuid(),
            ObjetivoId = goal.Id,
            Valor = request.Valor,
            Data = request.Data,
            Observacao = string.IsNullOrWhiteSpace(request.Observacao) ? null : request.Observacao.Trim(),
            Recorrente = request.Recorrente,
            CriadoEm = DateTime.UtcNow
        };

        await _dbContext.ObjetivoAportes.AddAsync(aporte, ct);
        goal.Aportes.Add(aporte);

        goal.AtualizadaEm = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        return StatusCode(StatusCodes.Status201Created, MapGoal(goal));
    }

    /// <summary>
    /// Lista o historico de depositos (aportes) de um objetivo.
    /// </summary>
    /// <param name="id">Id do objetivo</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Lista de depositos do objetivo</returns>
    [HttpGet("aporte/list")]
    [ProducesResponseType(typeof(V2GoalAporteListResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListAportes([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        var goalExists = await _dbContext.Objetivos
            .AsNoTracking()
            .AnyAsync(o => o.Id == id && o.UserId == userId, ct);

        if (!goalExists)
            return this.NotFoundError("Objetivo nao encontrado.");

        var aportes = await _dbContext.ObjetivoAportes
            .AsNoTracking()
            .Where(a => a.ObjetivoId == id)
            .OrderByDescending(a => a.Data)
            .Select(a => new V2GoalAporteResult(a.Id, a.Valor, a.Data, a.Observacao, a.Recorrente, a.CriadoEm, a.TransacaoId))
            .ToListAsync(ct);

        return Ok(new V2GoalAporteListResult(aportes));
    }

    /// <summary>
    /// Remove um deposito (aporte) manual do historico de um objetivo do usuario autenticado.
    /// Depositos originados de uma receita vinculada nao podem ser removidos por aqui: e preciso
    /// editar ou remover a transacao de origem.
    /// </summary>
    /// <param name="id">Id do deposito (aporte)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Objetivo atualizado</returns>
    [HttpDelete("aporte/remove")]
    [ProducesResponseType(typeof(V2GoalResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ResponseError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAporte([FromQuery] Guid id, CancellationToken ct)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return this.UnauthorizedError("Usuario autenticado invalido.");

        var aporte = await _dbContext.ObjetivoAportes
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (aporte is null)
            return this.NotFoundError("Deposito nao encontrado.");

        var goal = await _dbContext.Objetivos
            .Include(o => o.Carteira)
            .Include(o => o.Aportes)
            .FirstOrDefaultAsync(o => o.Id == aporte.ObjetivoId && o.UserId == userId, ct);

        if (goal is null)
            return this.NotFoundError("Deposito nao encontrado.");

        if (aporte.TransacaoId.HasValue)
            return this.BadRequestError("Este deposito veio de uma receita. Edite ou remova a transacao de origem.");

        _dbContext.ObjetivoAportes.Remove(aporte);
        goal.Aportes.Remove(aporte);

        goal.AtualizadaEm = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        return Ok(MapGoal(goal));
    }

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        return Guid.TryParse(userIdValue, out userId);
    }

    private async Task<string?> ValidateGoalRequestAsync(
        Guid userId,
        string nome,
        decimal valorTotal,
        int meses,
        Guid? carteiraId,
        string? iconKey,
        CancellationToken ct)
    {
        var trimmedName = nome?.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
            return "Nome do objetivo e obrigatorio.";

        if (trimmedName.Length > 120)
            return "Nome do objetivo deve ter no maximo 120 caracteres.";

        if (valorTotal <= 0)
            return "Valor total deve ser maior que zero.";

        if (meses <= 0)
            return "Meses deve ser maior que zero.";

        if (carteiraId.HasValue && !await WalletBelongsToUserAsync(userId, carteiraId.Value, ct))
            return "Carteira informada nao pertence ao usuario autenticado.";

        if (!string.IsNullOrWhiteSpace(iconKey) && !AllowedIconKeys.Contains(iconKey.Trim()))
            return "Icone invalido.";

        return null;
    }

    private Task<bool> WalletBelongsToUserAsync(Guid userId, Guid carteiraId, CancellationToken ct)
        => _dbContext.Carteiras
            .AsNoTracking()
            .AnyAsync(c => c.Id == carteiraId && c.UserId == userId, ct);

    private static string NormalizeIconKey(string? iconKey)
    {
        if (string.IsNullOrWhiteSpace(iconKey))
            return DefaultIconKey;

        return iconKey.Trim().ToLowerInvariant();
    }

    private static decimal ComputeMonthlyAmount(decimal totalAmount, int months)
        => decimal.Round(totalAmount / months, 2, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Meses restantes ate a data-alvo (CriadaEm + Meses), no mesmo criterio de calendario
    /// usado pelo frontend (date-fns differenceInCalendarMonths: apenas ano/mes, sem dia).
    /// Nunca retorna menos que 1 para evitar divisao por zero quando a meta vence no mes atual
    /// ou ja esta atrasada.
    /// </summary>
    private static int RemainingMonths(DateTime criadaEm, int meses)
    {
        var dataAlvo = criadaEm.AddMonths(meses);
        var now = DateTime.UtcNow;
        var calendarMonthsRemaining = ((dataAlvo.Year - now.Year) * 12) + (dataAlvo.Month - now.Month);

        return Math.Max(1, calendarMonthsRemaining);
    }

    private static V2GoalResult MapGoal(Objetivo objetivo)
    {
        var valorAportado = objetivo.Aportes.Sum(a => a.Valor);

        var valorRestante = objetivo.ValorTotal - valorAportado;
        if (valorRestante < 0)
            valorRestante = 0m;

        var percentualConcluido = objetivo.ValorTotal > 0
            ? decimal.Round((valorAportado / objetivo.ValorTotal) * 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        if (percentualConcluido > 100m)
            percentualConcluido = 100m;

        // Parcela ideal recalculada a cada leitura: cai conforme aportes reduzem o valor
        // restante, e sobe conforme o tempo passa sem aporte (menos meses restantes).
        var valorMensalIdeal = valorRestante > 0
            ? ComputeMonthlyAmount(valorRestante, RemainingMonths(objetivo.CriadaEm, objetivo.Meses))
            : 0m;

        return new(
            objetivo.Id,
            objetivo.Nome,
            NormalizeIconKey(objetivo.IconKey),
            objetivo.ValorTotal,
            objetivo.Meses,
            valorMensalIdeal,
            decimal.Round(valorAportado, 2, MidpointRounding.AwayFromZero),
            decimal.Round(valorRestante, 2, MidpointRounding.AwayFromZero),
            percentualConcluido,
            objetivo.CarteiraId,
            objetivo.Carteira?.Nome,
            objetivo.CriadaEm);
    }
}
