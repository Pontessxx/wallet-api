namespace AuthApi.Controllers.V2;

[ApiController]
[Route("user/v2")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserController : ControllerBase
{
	private const string RefreshTokenCookieName = "refreshToken";
	private readonly AuthV2Service _authService;

	public UserController(AuthV2Service authService)
	{
		_authService = authService;
	}

	/// <summary>
	/// Cria um novo usuário e já retorna a sessão autenticada.
	/// O refresh token é enviado em cookie HttpOnly.
	/// </summary>
	/// <param name="request">Dados do usuário</param>
	/// <param name="ct">Cancellation token</param>
	/// <returns>Access token e dados do usuário criado</returns>
	/// <response code="200">Usuário criado com sucesso</response>
	/// <response code="400">Dados inválidos</response>
	/// <response code="409">Usuário já existe</response>
	[HttpPost("create")]
	[AllowAnonymous]
	[ProducesResponseType(typeof(V2AuthSessionResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
	{
		try
		{
			var session = await _authService.RegisterAsync(request.Username, request.Password, GetClientIp(), ct);

			AppendRefreshTokenCookie(session.RefreshToken, session.RefreshTokenExpiresAt);

			return Ok(new V2AuthSessionResponse(
				session.AccessToken,
				session.ExpiresIn,
				session.User.Id,
				session.User.Username));
		}
		catch (InvalidOperationException ex)
		{
			return Conflict(new { message = ex.Message });
		}
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
}
