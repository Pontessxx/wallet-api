using System.IdentityModel.Tokens.Jwt;
using Application.Services;
using AuthApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Auth.Domain;

namespace AuthApi.Controllers;

[ApiController]
[Route("auth/v1")]
public class AuthController : ControllerBase
{
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
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "User";
            var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? request.Username;

            return Ok(new LoginResponse(Token: token, Role: role, Username: username));
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }
    }


    /// <summary>
    /// Atualiza a senha do usuário autenticado.
    /// </summary>
    /// <param name="request">Senha atual e nova senha</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Sem conteúdo</returns>
    /// <response code="204">Senha atualizada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autenticado</response>
    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] object request, CancellationToken ct)
    {
        // TODO
        return NoContent();
    }
}