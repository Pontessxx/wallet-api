namespace Wallet.Controllers;

[ApiController]
[Route("wallet/v1")]
[Authorize]
[ApiExplorerSettings(GroupName = "v1")]
public class WalletController : ControllerBase
{
    private readonly ContaCarteiraService _contaCarteiraService;

    public WalletController(ContaCarteiraService contaCarteiraService)
    {
        _contaCarteiraService = contaCarteiraService;
    }

    private Guid? GetAuthenticatedUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static CarteiraResponse MapCarteira(Carteira c) => new(
        c.Id, c.Descricao, c.SaldoInicial,
        c.Receitas, c.Despesas, c.Transferencias,
        c.Saldo, c.SaldoProjetado);

    private static ContaCarteiraResponse MapConta(ContaCarteira c) =>
        new(c.Id, c.Nome, c.Categoria.ToString(), MapCarteira(c.Carteira));

    // --- ContaCarteira --------------------------------------------------------

    /// <summary>
    /// Lista todas as contas carteira do usuario autenticado.
    /// </summary>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(IEnumerable<ContaCarteiraResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return Unauthorized(new { message = "Usuario autenticado invalido." });

        var contas = await _contaCarteiraService.GetByUserIdAsync(userId.Value, ct);
        return Ok(contas.Select(MapConta));
    }

    /// <summary>
    /// Retorna uma conta carteira pelo ID.
    /// </summary>
    [HttpGet("accounts/{id:guid}")]
    [ProducesResponseType(typeof(ContaCarteiraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return Unauthorized(new { message = "Usuario autenticado invalido." });

        var conta = await _contaCarteiraService.GetByIdAsync(id, ct);
        if (conta is null)
            return NotFound(new { message = "Conta carteira nao encontrada." });

        if (conta.UserId != userId.Value)
            return Forbid();

        return Ok(MapConta(conta));
    }

    /// <summary>
    /// Cria uma nova conta carteira para o usuario autenticado.
    /// </summary>
    [HttpPost("accounts")]
    [ProducesResponseType(typeof(ContaCarteiraResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreateContaCarteiraRequest request, CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return Unauthorized(new { message = "Usuario autenticado invalido." });

        if (!Enum.TryParse<WalletCategory>(request.Categoria, ignoreCase: true, out var categoria))
            return BadRequest(new { message = $"Categoria invalida. Valores aceitos: {string.Join(", ", Enum.GetNames<WalletCategory>())}." });

        var conta = await _contaCarteiraService.CreateAsync(
            userId.Value, request.Nome, categoria, request.Descricao, request.SaldoInicial, ct);

        return StatusCode(StatusCodes.Status201Created, MapConta(conta));
    }

    /// <summary>
    /// Remove uma conta carteira pelo ID.
    /// </summary>
    [HttpDelete("accounts/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return Unauthorized(new { message = "Usuario autenticado invalido." });

        try
        {
            await _contaCarteiraService.DeleteAsync(id, userId.Value, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    // --- Carteira (saldo) -----------------------------------------------------

    /// <summary>
    /// Retorna os dados de saldo da carteira de uma conta.
    /// </summary>
    [HttpGet("accounts/{id:guid}/carteira")]
    [ProducesResponseType(typeof(CarteiraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCarteira(Guid id, CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return Unauthorized(new { message = "Usuario autenticado invalido." });

        var conta = await _contaCarteiraService.GetByIdAsync(id, ct);
        if (conta is null)
            return NotFound(new { message = "Conta carteira nao encontrada." });

        if (conta.UserId != userId.Value)
            return Forbid();

        return Ok(MapCarteira(conta.Carteira));
    }

    /// <summary>
    /// Atualiza a descricao da carteira de uma conta.
    /// </summary>
    [HttpPatch("accounts/{id:guid}/carteira")]
    [ProducesResponseType(typeof(CarteiraResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCarteira(Guid id, [FromBody] UpdateCarteiraRequest request, CancellationToken ct)
    {
        var userId = GetAuthenticatedUserId();
        if (userId is null)
            return Unauthorized(new { message = "Usuario autenticado invalido." });

        try
        {
            await _contaCarteiraService.UpdateCarteiraAsync(id, userId.Value, request.Descricao, ct);

            var conta = await _contaCarteiraService.GetByIdAsync(id, ct);
            return Ok(MapCarteira(conta!.Carteira));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
