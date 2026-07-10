namespace AuthApi.Controllers.V2;

[ApiController]
[Route("auth/v2")]
[ApiExplorerSettings(GroupName = "v2")]
public class AuthController : ControllerBase
{
	private const string RefreshTokenCookieName = "refreshToken";
	private readonly AuthV2Service _authService;

	public AuthController(AuthV2Service authService)
	{
		_authService = authService;
	}

	/// <summary>
	/// Realiza o login do usuário e retorna o access token em JSON.
	/// O refresh token é enviado em cookie HttpOnly.
	/// </summary>
	/// <param name="ticketValidation">Header de validação com o TicketValidation</param>
	/// <param name="request">Credenciais do usuário</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Access token e dados do usuário autenticado</returns>
	/// <response code="200">Login realizado com sucesso</response>
	/// <response code="400">Header X-TicketValidation ausente ou inválido</response>
	/// <response code="401">Credenciais inválidas</response>
	[HttpPost("login")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(V2AuthSessionResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Login(
		[FromHeader(Name = "X-TicketValidation")] TicketValidationType ticketValidation,
		[FromBody] LoginRequest request,
		CancellationToken ct)
	{
		if (!IsValidJwtTicketValidation(ticketValidation))
			return BadRequest(new { message = "Header X-TicketValidation ausente ou inválido." });

		try
		{
			var session = await _authService.LoginAsync(request.Username, request.Password, ticketValidation, GetClientIp(), ct);

			AppendRefreshTokenCookie(session.RefreshToken, session.RefreshTokenExpiresAt);

			return Ok(new V2AuthSessionResponse(
				session.AccessToken,
				session.ExpiresIn,
				session.User.Id,
				session.User.Username));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(new { message = "Credenciais inválidas." });
		}
	}

	/// <summary>
	/// Atualiza o access token usando o refresh token do cookie HttpOnly
	/// e validando que ele pertence ao usuário autenticado no access token atual.
	/// </summary>
	/// <param name="ticketValidation">Header de validação com o TicketValidation</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Novo access token</returns>
	/// <response code="200">Token renovado com sucesso</response>
	/// <response code="400">Header X-TicketValidation ausente ou inválido</response>
	/// <response code="401">Access token inválido, refresh token inválido/expirado ou sem vínculo entre ambos</response>
	[HttpPost("refresh")]
	[Authorize]
	[ProducesResponseType(typeof(V2RefreshResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Refresh(
		[FromHeader(Name = "X-TicketValidation")] TicketValidationType ticketValidation,
		CancellationToken ct)
	{
		if (!IsValidJwtTicketValidation(ticketValidation))
			return BadRequest(new { message = "Header X-TicketValidation ausente ou inválido." });

		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Access token inválido ou sem claim de usuário." });

		if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
			return Unauthorized(new { message = "Refresh token ausente." });

		try
		{
			var session = await _authService.RefreshAsync(refreshToken, ticketValidation, userId, GetClientIp(), ct);

			AppendRefreshTokenCookie(session.RefreshToken, session.RefreshTokenExpiresAt);

			return Ok(new V2RefreshResponse(session.AccessToken, session.ExpiresIn));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(new { message = "Refresh token inválido ou expirado." });
		}
	}

	/// <summary>
	/// Valida se o refresh token atual é válido e pertence ao usuário autenticado.
	/// </summary>
	/// <param name="ticketValidation">Header de validação com o TicketValidation</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Status de validação do refresh token</returns>
	/// <response code="200">Refresh token válido para o usuário autenticado</response>
	/// <response code="400">Header X-TicketValidation ausente ou inválido</response>
	/// <response code="401">Token inválido, ausente, expirado ou sem vínculo com o usuário</response>
	[HttpGet("validate")]
	[Authorize]
	[ProducesResponseType(typeof(V2RefreshValidationResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Validate(
		[FromHeader(Name = "X-TicketValidation")] TicketValidationType ticketValidation,
		CancellationToken ct)
	{
		if (!IsValidJwtTicketValidation(ticketValidation))
			return BadRequest(new { message = "Header X-TicketValidation ausente ou inválido." });

		if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
			return Unauthorized(new { message = "Refresh token ausente." });

		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Access token inválido ou sem claim de usuário." });

		try
		{
			var validation = await _authService.ValidateRefreshOwnershipAsync(refreshToken, userId, ct);

			return Ok(new V2RefreshValidationResponse(
				validation.IsValid,
				validation.UserId,
				validation.ExpiresAt));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(new { message = "Refresh token inválido, expirado ou não pertence ao usuário autenticado." });
		}
	}

	/// <summary>
	/// Encerra a sessão do usuário autenticado, removendo o cookie de refresh token
	/// e revogando os refresh tokens ativos do usuário.
	/// </summary>
	/// <param name="ticketValidation">Header de validação com o TicketValidation</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Sem conteúdo quando o logout for processado</returns>
	/// <response code="204">Logout processado com sucesso</response>
	/// <response code="400">Header X-TicketValidation ausente ou inválido</response>
	/// <response code="401">Access token inválido ou sem claim de usuário</response>
	[HttpDelete("logout")]
	[Authorize]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Logout(
		[FromHeader(Name = "X-TicketValidation")] TicketValidationType ticketValidation,
		CancellationToken ct)
	{
		if (!IsValidJwtTicketValidation(ticketValidation))
			return BadRequest(new { message = "Header X-TicketValidation ausente ou inválido." });

		if (!TryGetAuthenticatedUserId(out var userId))
			return Unauthorized(new { message = "Access token inválido ou sem claim de usuário." });

		Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);

		await _authService.LogoutAsync(userId, refreshToken, GetClientIp(), ct);

		DeleteRefreshTokenCookie();

		return NoContent();
	}

	private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

	private void AppendRefreshTokenCookie(string refreshToken, DateTime expiresAt)
	{
		Response.Cookies.Append(
			RefreshTokenCookieName,
			refreshToken,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Path = "/",
				Expires = expiresAt
			});
	}

	private void DeleteRefreshTokenCookie()
	{
		Response.Cookies.Delete(
			RefreshTokenCookieName,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Path = "/"
			});
	}

	private bool IsValidJwtTicketValidation(TicketValidationType ticketValidation)
	{
		if (!Request.Headers.TryGetValue("X-TicketValidation", out var rawHeaderValue))
			return false;

		if (!Enum.TryParse<TicketValidationType>(rawHeaderValue.ToString(), true, out var parsedHeaderValue))
			return false;

		return ticketValidation == parsedHeaderValue && ticketValidation == TicketValidationType.JwtOnly;
	}

	private bool TryGetAuthenticatedUserId(out Guid userId)
	{
		var userIdValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
			?? User.FindFirstValue(ClaimTypes.NameIdentifier);

		return Guid.TryParse(userIdValue, out userId);
	}
}