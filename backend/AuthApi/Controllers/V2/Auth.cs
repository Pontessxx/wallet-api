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
	/// <param name="request">Credenciais do usuário</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Access token e dados do usuário autenticado</returns>
	/// <response code="200">Login realizado com sucesso</response>
	/// <response code="401">Credenciais inválidas</response>
	[HttpPost("login")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(V2AuthSessionResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
	{
		try
		{
			var session = await _authService.LoginAsync(request.Username, request.Password, GetClientIp(), ct);

			AppendRefreshTokenCookie(session.RefreshToken, session.RefreshTokenExpiresAt);

			return Ok(new V2AuthSessionResponse(
				session.AccessToken,
				session.ExpiresIn,
				new V2AuthenticatedUserResponse(session.User.Id, session.User.Username)));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(new { message = "Credenciais inválidas." });
		}
	}

	/// <summary>
	/// Atualiza o access token usando apenas o refresh token do cookie HttpOnly.
	/// </summary>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Novo access token</returns>
	/// <response code="200">Token renovado com sucesso</response>
	/// <response code="401">Refresh token inválido ou expirado</response>
	[HttpPost("refresh")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(V2RefreshResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Refresh(CancellationToken ct)
	{
		if (!Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
			return Unauthorized(new { message = "Refresh token ausente." });

		try
		{
			var session = await _authService.RefreshAsync(refreshToken, GetClientIp(), ct);

			AppendRefreshTokenCookie(session.RefreshToken, session.RefreshTokenExpiresAt);

			return Ok(new V2RefreshResponse(session.AccessToken, session.ExpiresIn));
		}
		catch (UnauthorizedAccessException)
		{
			return Unauthorized(new { message = "Refresh token inválido ou expirado." });
		}
	}

	/// <summary>
	/// Remove o refresh token atual e encerra a sessão.
	/// </summary>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Sem conteúdo</returns>
	/// <response code="204">Logout realizado com sucesso</response>
	[HttpPost("logout")]
	[AllowAnonymous]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public async Task<IActionResult> Logout(CancellationToken ct)
	{
		if (Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
		{
			try
			{
				await _authService.LogoutAsync(refreshToken, GetClientIp(), ct);
			}
			catch (UnauthorizedAccessException)
			{
				// Logout deve ser idempotente.
			}
		}

		ExpireRefreshTokenCookie();
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

	private void ExpireRefreshTokenCookie()
	{
		Response.Cookies.Append(
			RefreshTokenCookieName,
			string.Empty,
			new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.Strict,
				Path = "/",
				Expires = DateTimeOffset.UnixEpoch
			});
	}
}