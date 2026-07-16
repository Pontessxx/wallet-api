namespace AuthApi.Controllers.V2;

[ApiController]
[Route("goal/v2")]
[Authorize]
[ApiExplorerSettings(GroupName = "v2")]
public class GoalController : ControllerBase
{
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

        var validationError = await ValidateGoalRequestAsync(userId, request.Nome, request.ValorTotal, request.Meses, carteiraId, ct);
        if (validationError is not null)
            return this.BadRequestError(validationError);

        var goal = new Objetivo
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CarteiraId = carteiraId,
            Nome = request.Nome.Trim(),
            ValorTotal = request.ValorTotal,
            Meses = request.Meses,
            ValorMensal = ComputeMonthlyAmount(request.ValorTotal, request.Meses),
            AporteManualAcumulado = 0m,
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

        var validationError = await ValidateGoalRequestAsync(userId, request.Nome, request.ValorTotal, request.Meses, request.CarteiraId, ct);
        if (validationError is not null)
            return this.BadRequestError(validationError);

        var goal = await _dbContext.Objetivos
            .Include(o => o.Carteira)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

        if (goal is null)
            return this.NotFoundError("Objetivo nao encontrado.");

        goal.Nome = request.Nome.Trim();
        goal.ValorTotal = request.ValorTotal;
        goal.Meses = request.Meses;
        goal.CarteiraId = request.CarteiraId;
        goal.ValorMensal = ComputeMonthlyAmount(request.ValorTotal, request.Meses);

        if (request.AporteManual.HasValue)
        {
            if (request.AporteManual.Value <= 0)
                return this.BadRequestError("Valor do aporte manual deve ser maior que zero.");

            if (goal.CarteiraId.HasValue)
                return this.BadRequestError("Objetivos atrelados a carteira usam saldo da carteira e nao aceitam aporte manual.");

            goal.AporteManualAcumulado += request.AporteManual.Value;
        }

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

        return null;
    }

    private Task<bool> WalletBelongsToUserAsync(Guid userId, Guid carteiraId, CancellationToken ct)
        => _dbContext.Carteiras
            .AsNoTracking()
            .AnyAsync(c => c.Id == carteiraId && c.UserId == userId, ct);

    private static decimal ComputeMonthlyAmount(decimal totalAmount, int months)
        => decimal.Round(totalAmount / months, 2, MidpointRounding.AwayFromZero);

    private static V2GoalResult MapGoal(Objetivo objetivo)
    {
        var usaAporteManual = !objetivo.CarteiraId.HasValue;
        var valorAportado = usaAporteManual
            ? objetivo.AporteManualAcumulado
            : (objetivo.Carteira?.Saldo ?? 0m);

        if (valorAportado < 0)
            valorAportado = 0m;

        var valorRestante = objetivo.ValorTotal - valorAportado;
        if (valorRestante < 0)
            valorRestante = 0m;

        var percentualConcluido = objetivo.ValorTotal > 0
            ? decimal.Round((valorAportado / objetivo.ValorTotal) * 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;

        if (percentualConcluido > 100m)
            percentualConcluido = 100m;

        return new(
            objetivo.Id,
            objetivo.Nome,
            objetivo.ValorTotal,
            objetivo.Meses,
            objetivo.ValorMensal,
            decimal.Round(valorAportado, 2, MidpointRounding.AwayFromZero),
            decimal.Round(valorRestante, 2, MidpointRounding.AwayFromZero),
            percentualConcluido,
            usaAporteManual,
            objetivo.CarteiraId);
    }
}
