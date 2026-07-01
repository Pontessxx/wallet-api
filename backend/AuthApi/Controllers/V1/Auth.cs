namespace AuthApi.Controllers;

[ApiController]
[Route("auth/v1")]
public class AuthController : ControllerBase
{
    private const int ResetCodeLength = 6;
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Verifica se a API está saudável.
    /// </summary>
    /// <returns>Status da aplicação</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Get() => Ok(new { status = "healthy" });

    /// <summary>
    /// Realiza o login do usuário e retorna um token JWT.
    /// </summary>
    /// <param name="ticketValidation">Header de validação com o TicketValidation</param>
    /// <param name="request">Credenciais do usuário</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Token JWT e role do usuário</returns>
    /// <response code="200">Login realizado com sucesso</response>
    /// <response code="400">Header X-TicketValidation ausente ou inválido</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromHeader(Name = "X-TicketValidation")] TicketValidationType ticketValidation,
        LoginRequest request,
        CancellationToken ct)
    {
        if (ticketValidation != TicketValidationType.JwtOnly)
            return BadRequest(new { message = "Header X-TicketValidation inválido." });

        try
        {
            var token = await _authService.LoginAsync(request.Username, request.Password, ct);

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var userIdValue = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "User";
            var username = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value
                ?? jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                ?? request.Username;

            _ = Guid.TryParse(userIdValue, out var userId);

            return Ok(new LoginResponse(Id: userId, Token: token, Role: role, Username: username));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }
    }

    /// <summary>
    /// Gera um código numérico temporário para redefinição de senha.
    /// </summary>
    /// <param name="request">Nome do usuário que vai receber o código</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Código de redefinição e prazo de expiração</returns>
    /// <response code="200">Código gerado com sucesso</response>
    [HttpPost("reset-code")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PasswordResetCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateResetCode([FromBody] ResetCodeRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return BadRequest(new { message = "Username é obrigatório." });

        var result = await _authService.GeneratePasswordResetCodeAsync(request.Username, ct);

        return Ok(new PasswordResetCodeResponse(result.Code, result.ExpiresAt));
    }


    /// <summary>
    /// Atualiza a senha do usuário informado no body usando username, código de reset e nova senha.
    /// </summary>
    /// <param name="request">Nome do usuário, código de redefinição e nova senha</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Mensagem de sucesso</returns>
    /// <response code="200">Senha atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autenticado</response>
    [HttpPut("change-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        try
        {
            await _authService.ChangePasswordAsync(request.Username, request.ResetCode, request.NewPassword, ct);

            return Ok(new ChangePasswordResponse("Senha atualizada com sucesso."));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Código de redefinição inválido ou expirado." });
        }
    }
}