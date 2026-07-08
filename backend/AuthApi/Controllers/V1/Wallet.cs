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

	/// <summary>
	/// Lista todas as contas carteira do usuário autenticado.
	/// </summary>
	[HttpGet("accounts")]
	[ProducesResponseType(typeof(WalletAccountsResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetAll(CancellationToken ct)
	{
		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Usuário autenticado inválido." });

		var carteiras = await _contaCarteiraService.GetAllAsync(userId, ct);
		return Ok(carteiras);
	}

	/// <summary>
	/// Cria uma nova conta carteira para o usuário autenticado.
	/// </summary>
	[HttpPost("accounts/create")]
	[ProducesResponseType(typeof(CarteiraResult), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Create(
		[FromBody] CreateCarteiraRequest request,
		[FromHeader(Name = "X-WalletType")] WalletCategory walletType,
		CancellationToken ct)
	{
		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Usuário autenticado inválido." });

		if (!IsValidWalletTypeHeader(walletType))
		{
			var allowedValues = string.Join(", ", Enum.GetNames<WalletCategory>());
			return BadRequest(new { message = $"Header X-WalletType inválido. Valores permitidos: {allowedValues}." });
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
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Remove uma conta carteira pelo ID para o usuário autenticado.
	/// </summary>
	[HttpDelete("accounts/remove")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Delete([FromBody] RemoveCarteiraRequest request, CancellationToken ct)
	{
		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Usuário autenticado inválido." });

		try
		{
			await _contaCarteiraService.DeleteAsync(userId, request.Id, ct);
			return NoContent();
		}
		catch (InvalidOperationException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Atualiza nome e categoria de uma conta carteira do usuário autenticado.
	/// </summary>
	[HttpPut("accounts/edit")]
	[ProducesResponseType(typeof(CarteiraResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Edit([FromBody] EditCarteiraRequest request, CancellationToken ct)
	{
		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Usuário autenticado inválido." });

		try
		{
			var carteira = await _contaCarteiraService.UpdateAsync(userId, request.Id, request.Nome, request.Categoria, ct);
			return Ok(carteira);
		}
		catch (InvalidOperationException ex) when (ex.Message.Contains("não encontrada", StringComparison.OrdinalIgnoreCase))
		{
			return NotFound(new { message = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
	}

	/// <summary>
	/// Retorna o resumo consolidado das carteiras do usuário autenticado.
	/// </summary>
	[HttpGet("summary")]
	[ProducesResponseType(typeof(WalletSummaryResult), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> GetSummary(CancellationToken ct)
	{
		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Usuário autenticado inválido." });

		var summary = await _contaCarteiraService.GetSummaryAsync(userId, ct);
		return Ok(summary);
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
